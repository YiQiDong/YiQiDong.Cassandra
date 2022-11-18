using Newtonsoft.Json;
using Quick.Fields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using YiQiDong.Cassandra.Model;
using YiQiDong.Cassandra.Utils;
using YiQiDong.Core;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra.Functions
{
    class UserManage : AbstractFunction
    {
        private const string USERINFOS_FILE = "YiQiDong.Cassandra.Model.UserInfos.json";

        private const string DELETE_USER_ID_PREFIX = "DELETE_USER_ID_PREFIX_";

        public override string Name => "用户管理";
        public static UserManage Instance { get; private set; }

        private string imageFolder;
        private List<UserInfo> _userInfoList;
        private List<UserInfo> GetUserInfoList()
        {
            if (_userInfoList == null)
            {
                var containerUserInfosFile = Path.Combine(Config.Instance.GetDataFolder(), USERINFOS_FILE);
                if (!File.Exists(containerUserInfosFile))
                {
                    var folder = Path.GetDirectoryName(containerUserInfosFile);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    var imageUserInfosFile = Path.Combine(imageFolder, USERINFOS_FILE);
                    if (File.Exists(imageUserInfosFile))
                        File.Copy(imageUserInfosFile, containerUserInfosFile, true);
                }

                var userInfoFile = Path.Combine(Config.Instance.GetDataFolder(), USERINFOS_FILE);
                var userInfoContent = File.ReadAllText(userInfoFile);
                _userInfoList = JsonConvert.DeserializeObject<List<UserInfo>>(userInfoContent);
            }
            return _userInfoList;
        }

        public UserManage(string imageFolder)
        {
            Instance = this;
            this.imageFolder = imageFolder;
        }

        private void SaveUserInfos()
        {
            var userInfoFile = Path.Combine(Config.Instance.GetDataFolder(), USERINFOS_FILE);
            File.WriteAllText(userInfoFile, JsonConvert.SerializeObject(GetUserInfoList()), Encoding.UTF8);
        }

        private List<FieldForGet> innerGet(FunctionRequest request)
        {
            List<FieldForGet> list = new List<FieldForGet>();
            //当容器未启动时，此功能不可用
            if (!Agent.Instance.ContainerInfo.AutoStart)
            {
                list.Add(new FieldForGet() { Name = "当前功能不可用", Description = $"容器尚未启动，该功能不可用，请先启动容器，然后再试。", Type = FieldType.Alert });
                return list;
            }
            //如果认证方式未配置为“密码认证”，则此功能不可用
            var properties = Config.Instance.Properties;
            if (properties["authenticator"] != "PasswordAuthenticator")
            {
                list.Add(new FieldForGet() { Name = "当前功能不可用", Description = $"认证方式未配置为'密码认证'，当前功能不可用。", Type = FieldType.Alert });
                return list;
            }
            list.Add(new FieldForGet() { Name = "新增用户", Type = FieldType.ContainerGroup });
            list.Add(new FieldForGet()
            {
                Id = "User",
                Name = "用户名",
                Type = FieldType.InputText,
                Input_AllowBlank = false,
                Value = request == null ? "test" : request.GetFieldValue("User")
            });
            list.Add(new FieldForGet()
            {
                Id = "Password",
                Name = "密码",
                Type = FieldType.InputText,
                Input_AllowBlank = false,
                Value = request == null ? "test" : request.GetFieldValue("Password")
            });
            list.Add(new FieldForGet() { Id = "AddUser", Name = "添加", Type = FieldType.Button });

            var connectInfo = Config.Instance.GetConnectInfo();
            var connectUserInfo = GetConnectUserInfo();

            DbUtils.UseSession(connectInfo.Address.ToString(), connectInfo.Port, connectUserInfo?.Name, connectUserInfo?.Password, session =>
             {
                 var rs = session.Execute("LIST ROLES;");
                 foreach (var row in rs)
                 {
                     var name = row[0].ToString();
                     list.Add(new FieldForGet() { Name = $"用户[{name}]", Type = FieldType.ContainerGroup });
                     var password = GetUserInfoList().FirstOrDefault(t => t.Name == name)?.Password;
                     if (!string.IsNullOrEmpty(password))
                         list.Add(new FieldForGet() { Name = "密码", Value = password, Type = FieldType.InputText, Input_ReadOnly = true });
                     list.Add(new FieldForGet() { Id = DELETE_USER_ID_PREFIX + name, Name = "删除", Type = FieldType.Button });
                 }
             });
            return list;
        }

        public override FieldForGet[] Get()
        {
            return innerGet(null).ToArray();
        }

        public UserInfo GetConnectUserInfo(string noUseAccount = null)
        {
            var properties = Config.Instance.Properties;
            if (properties["authenticator"] == "PasswordAuthenticator")
            {
                var userInfo = GetUserInfoList().FirstOrDefault(t => noUseAccount == null || t.Name != noUseAccount);
                if (userInfo == null && noUseAccount == null)
                    throw new ApplicationException($"配置了使用密码认证，但未找到用户信息！");
                return userInfo;
            }
            return null;
        }

        public override FieldForGet[] Post(FunctionRequest request)
        {
            var connectInfo = Config.Instance.GetConnectInfo();
            var connectUserInfo = GetConnectUserInfo();

            var list = new Lazy<List<FieldForGet>>(() => innerGet(request));

            //如果是添加用户
            if (request.IsFieldIdsMatch("AddUser"))
            {
                var newUserInfo = new UserInfo()
                {
                    Name = request.GetFieldValue("User"),
                    Password = request.GetFieldValue("Password")
                };

                FieldForGet retField = null;
                try
                {
                    DbUtils.UseSession(connectInfo.Address.ToString(), connectInfo.Port, connectUserInfo?.Name, connectUserInfo?.Password, session =>
                    {
                        var rs = session.Execute($"CREATE ROLE {newUserInfo.Name} WITH PASSWORD = '{newUserInfo.Password}' AND LOGIN = true AND SUPERUSER = true;");
                        retField = new FieldForGet() { Name = "添加成功", Description = $"已成功添加用户[{newUserInfo.Name}]", Type = FieldType.MessageBox };
                        GetUserInfoList().Add(newUserInfo);
                        SaveUserInfos();
                    });
                }
                catch (Exception ex)
                {
                    retField = new FieldForGet() { Name = "添加失败", Description = $"添加用户[{newUserInfo.Name}]失败，原因：{ex.Message}", Type = FieldType.Alert };
                }
                if (retField.Type == FieldType.Alert)
                    list.Value.Insert(0, retField);
                else
                    list.Value.Add(retField);
            }
            else if (request.FieldIds[0].StartsWith(DELETE_USER_ID_PREFIX))
            {
                FieldForGet retField = null;                
                var toDeluser = request.FieldIds[0].Substring(DELETE_USER_ID_PREFIX.Length);
                var toDelUserInfo = GetUserInfoList().FirstOrDefault(t => t.Name == toDeluser);

                try
                {
                    connectUserInfo = GetConnectUserInfo(toDeluser);
                    if (connectUserInfo == null)
                        throw new ApplicationException("至少要保留一个用户！");
                    DbUtils.UseSession(connectInfo.Address.ToString(), connectInfo.Port, connectUserInfo?.Name, connectUserInfo?.Password, session =>
                    {
                        var rs = session.Execute($"DROP ROLE IF EXISTS {toDeluser};");
                        retField = new FieldForGet() { Name = "删除成功", Description = $"已成功删除用户[{toDeluser}]", Type = FieldType.MessageBox };
                        if (toDelUserInfo != null)
                        {
                            GetUserInfoList().Remove(toDelUserInfo);
                            SaveUserInfos();
                        }
                    });
                }
                catch (Exception ex)
                {
                    retField = new FieldForGet() { Name = "删除失败", Description = $"删除用户[{toDeluser}]失败，原因：{ex.Message}", Type = FieldType.Alert };
                }
                if (retField.Type == FieldType.Alert)
                    list.Value.Insert(0, retField);
                else
                    list.Value.Add(retField);
            }
            return list.Value.ToArray();
        }
    }
}
