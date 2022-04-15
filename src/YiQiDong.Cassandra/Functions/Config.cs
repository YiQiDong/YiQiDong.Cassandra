using Quick.Fields;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using YiQiDong.Cassandra.Utils;
using YiQiDong.Core;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra.Functions
{
    class Config : AbstractFunction
    {
        public const string CONFIG_FILE = "conf/cassandra.yaml";
        public static Config Instance { get; private set; }
        private string containerFolder;

        public override string Name => "配置";
        public Dictionary<string, string> Properties = null;
        private string containerConfigFile;
        public Config(string imageFolder, string containerFolder)
        {
            Instance = this;
            this.containerFolder = containerFolder;
            containerConfigFile = Path.Combine(containerFolder, CONFIG_FILE);

            if (!File.Exists(containerConfigFile))
            {
                var folder = Path.GetDirectoryName(containerConfigFile);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                var imageConfigFile = Path.Combine(imageFolder, CONFIG_FILE);
                if (File.Exists(imageConfigFile))
                    File.Copy(imageConfigFile, containerConfigFile, true);
            }
            RefreshProperties();
        }

        public void RefreshProperties()
        {
            if (File.Exists(containerConfigFile))
                Properties = YamlFileUtils.Load(containerConfigFile);
        }

        public IPEndPoint GetConnectInfo()
        {
            if (Properties == null)
                throw new ApplicationException($"配置文件[{CONFIG_FILE}]不存在！");
            var host = Properties["rpc_address"];
            if (host == "0.0.0.0")
                host = "127.0.0.1";

            var port = int.Parse(Properties["native_transport_port"]);
            return new IPEndPoint(IPAddress.Parse(host), port);
        }

        private List<FieldForGet> innerGet(FunctionRequest request, bool isReadOnly = false)
        {
            List<FieldForGet> list = new List<FieldForGet>();
            if (!File.Exists(containerConfigFile))
            {
                list.Add(new FieldForGet() { Name = "失败", Description = $"配置文件[{CONFIG_FILE}]不存在！", Type = FieldType.Alert });
                return list;
            }
            List<FieldForGet> spliterFieldChildren = new List<FieldForGet>();
            List<FieldForGet> basicFieldChildren = new List<FieldForGet>();
            List<FieldForGet> securityFieldChildren = new List<FieldForGet>();
            List<FieldForGet> clusterFieldChildren = new List<FieldForGet>();

            string tmpKey;
            tmpKey = "rpc_address";
            if (Properties.ContainsKey(tmpKey))
                basicFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "rpc_address",
                    Type = FieldType.InputText,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Basic", tmpKey),
                    Input_AllowBlank = false,
                    Input_RegularExpression = @"^((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})(\.((2(5[0-5]|[0-4]\d))|[0-1]?\d{1,2})){3}$",
                    Description = "rpc_address: Cassandra的监听IP地址，要监听全部IP地址时使用:0.0.0.0"
                });

            tmpKey = "native_transport_port";
            if (Properties.ContainsKey(tmpKey))
                basicFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "native_transport_port",
                    Type = FieldType.InputNumber,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Basic", tmpKey),
                    Input_AllowBlank = false,
                    Input_RegularExpression = "^[1-9]$|(^[1-9][0-9]$)|(^[1-9][0-9][0-9]$)|(^[1-9][0-9][0-9][0-9]$)|(^[1-6][0-5][0-5][0-3][0-5]$)",
                    Description = "native_transport_port: Cassandra的监听端口，默认为9042"
                });
            spliterFieldChildren.Add(new FieldForGet()
            {
                Id = "Basic",
                Name = "基本",
                Type = FieldType.ContainerGroup,
                Children = basicFieldChildren.ToArray()
            });


            tmpKey = "authenticator";
            if (Properties.ContainsKey(tmpKey))
                securityFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "认证方式",
                    Type = FieldType.InputSelect,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Security", tmpKey),
                    Input_AllowBlank = false,
                    InputSelect_Options = new Dictionary<string, string>()
                    {
                        ["AllowAllAuthenticator"] = "无",
                        ["PasswordAuthenticator"] = "密码认证"
                    }
                });
            spliterFieldChildren.Add(new FieldForGet()
            {
                Id = "Security",
                Name = "安全",
                Type = FieldType.ContainerGroup,
                Children = securityFieldChildren.ToArray()
            });

            tmpKey = "cluster_name";
            if (Properties.ContainsKey(tmpKey))
                clusterFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "群集名称",
                    Type = FieldType.InputText,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Cluster", tmpKey),
                    Input_AllowBlank = false,
                    Description = "默认为：'Test Cluster'"
                });

            tmpKey = "          - seeds";
            if (Properties.ContainsKey(tmpKey))
                clusterFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "种子节点",
                    Type = FieldType.InputText,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Cluster", tmpKey),
                    Input_AllowBlank = false,
                    Description = "每个种子节点的内部IP，多个种子节点IP由逗号分隔。默认为：\"127.0.0.1\""
                });

            tmpKey = "listen_address";
            if (Properties.ContainsKey(tmpKey))
                clusterFieldChildren.Add(new FieldForGet()
                {
                    Id = tmpKey,
                    Name = "listen_address",
                    Type = FieldType.InputText,
                    Input_ReadOnly = isReadOnly,
                    Value = request == null ? Properties[tmpKey] : request.GetFieldValue("tab", "Cluster", tmpKey),
                    Input_AllowBlank = false,
                    Description = "listen_address: 如果配置了群集，必须修改此参数。不能使用0.0.0.0。默认为：localhost"
                });

            spliterFieldChildren.Add(new FieldForGet()
            {
                Id = "Cluster",
                Name = "群集",
                Type = FieldType.ContainerGroup,
                Children = clusterFieldChildren.ToArray()
            });

            list.Add(new FieldForGet()
            {
                Id = "tab",
                Type = FieldType.ContainerTab,
                Children = spliterFieldChildren.ToArray()
            });
            return list;
        }

        public override FieldForGet[] Get()
        {
            var isReadOnly = Agent.Instance.ContainerInfo.AutoStart;
            var list = innerGet(null, isReadOnly);
            if (!isReadOnly)
                addSaveButton(list);
            return list.ToArray();
        }

        private void travelFields(FieldForPost[] fields, Action<FieldForPost> action)
        {
            if (fields == null)
                return;
            foreach (var field in fields)
            {
                action(field);
                travelFields(field.Children, action);
            }
        }

        private Dictionary<string, string> getDictFromFields(FieldForPost[] fields)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            travelFields(fields, field =>
            {
                if (string.IsNullOrEmpty(field.Id))
                    return;
                dict[field.Id] = field.Value;
            });
            return dict;
        }

        public override FieldForGet[] Post(FunctionRequest request)
        {
            var list = innerGet(request);
            if (request.IsFieldIdsMatch("Save"))
            {
                if (File.Exists(containerConfigFile))
                {
                    YamlFileUtils.Save(containerConfigFile, getDictFromFields(request.Fields));
                    //保存成功后重新加载配置文件
                    RefreshProperties();
                    list.Add(new FieldForGet()
                    {
                        Name = "保存成功",
                        Description = $"配置文件[{CONFIG_FILE}]保存成功！",
                        Type = FieldType.MessageBox
                    });
                }
                else
                {
                    list.Add(new FieldForGet()
                    {
                        Name = "错误",
                        Description = $"配置文件[{CONFIG_FILE}]不存在！",
                        Type = FieldType.Alert
                    });
                }
                addSaveButton(list);
            }
            return list.ToArray();
        }

        private void addSaveButton(List<FieldForGet> list)
        {
            list.Add(new FieldForGet() { Id = "Save", Name = "保存", Type = FieldType.Button });
        }
    }
}
