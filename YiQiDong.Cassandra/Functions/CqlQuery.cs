using System;
using System.Collections.Generic;
using System.Text;
using YiQiDong.Protocol.V1.Model;
using Cassandra;
using System.Linq;
using YiQiDong.Cassandra.Utils;
using System.Collections;
using Quick.Fields;
using YiQiDong.Core;

namespace YiQiDong.Cassandra.Functions
{
    public class CqlQuery : AbstractFunction
    {
        public override string Name => "CQL查询";

        private List<FieldForGet> innerGet(FunctionRequest request)
        {
            List<FieldForGet> list = new List<FieldForGet>();

            List<FieldForGet> spliterFieldChildren = new List<FieldForGet>();
            List<FieldForGet> connectionFieldChildren = new List<FieldForGet>();

            connectionFieldChildren.Add(new FieldForGet()
            {
                Id = "ConnectTo",
                Name = "连接到",
                Type = FieldType.InputSelect,
                Input_AllowBlank = false,
                PostOnChanged = true,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["Self"] = "当前容器",
                    ["Other"] = "其他服务"
                },
                Value = request == null ? "Self" : request.GetFieldValue("tab", "ConnectionInfo", "ConnectTo")
            });
            if (request != null && request.GetFieldValue("ConnectTo") != "Self")
            {
                connectionFieldChildren.Add(new FieldForGet()
                {
                    Id = "Host",
                    Name = "主机",
                    Type = FieldType.InputText,
                    Input_AllowBlank = false,
                    Value = request == null ? "127.0.0.1" : request.GetFieldValue("tab", "ConnectionInfo", "Host")
                });
                connectionFieldChildren.Add(new FieldForGet()
                {
                    Id = "Port",
                    Name = "端口",
                    Type = FieldType.InputNumber,
                    Input_AllowBlank = false,
                    Input_RegularExpression = "^[1-9]$|(^[1-9][0-9]$)|(^[1-9][0-9][0-9]$)|(^[1-9][0-9][0-9][0-9]$)|(^[1-6][0-5][0-5][0-3][0-5]$)",
                    Value = request == null ? "9043" : request.GetFieldValue("tab", "ConnectionInfo", "Port")
                });
                connectionFieldChildren.Add(new FieldForGet()
                {
                    Id = "User",
                    Name = "用户",
                    Type = FieldType.InputText,
                    Input_AllowBlank = true,
                    Value = request == null ? null : request.GetFieldValue("tab", "ConnectionInfo", "User")
                });
                connectionFieldChildren.Add(new FieldForGet()
                {
                    Id = "Password",
                    Name = "密码",
                    Type = FieldType.InputText,
                    Input_AllowBlank = true,
                    Value = request == null ? null : request.GetFieldValue("tab", "ConnectionInfo", "Password")
                });
            }
            spliterFieldChildren.Add(new FieldForGet()
            {
                Id = "ConnectionInfo",
                Name = "连接信息",
                Type = FieldType.ContainerGroup,
                Children = connectionFieldChildren.ToArray()
            });

            spliterFieldChildren.Add(new FieldForGet()
            {
                Id = "Query",
                Name = "查询",
                Type = FieldType.ContainerGroup,
                Children = new FieldForGet[]
                {
                    new FieldForGet()
                    {
                        Id = "Script",
                        Name = "脚本",
                        Type = FieldType.InputTextArea,
                        InputTextArea_Rows = 8,
                        Input_AllowBlank = true,
                        Value = request == null ? null : request.GetFieldValue("tab", "Query","Script")
                    },
                    new FieldForGet() { Id = "Execute", Name = "执行", Type = FieldType.Button }
                }
            });

            list.Add(new FieldForGet()
            {
                Id = "tab",
                Type = FieldType.ContainerTab,
                Children = spliterFieldChildren.ToArray()
            });
            return list;
        }

        public override FieldForGet[] Execute(FunctionRequest request)
        {
            if (request == null)
                return Get();
            return Post(request);
        }

        public FieldForGet[] Get()
        {
            return innerGet(null).ToArray();
        }

        public FieldForGet[] Post(FunctionRequest request)
        {
            var list = innerGet(request);

            if (request.IsFieldIdsMatch("tab", "Query", "Execute"))
            {
                var script = request.GetFieldValue("tab", "Query", "Script");
                if (string.IsNullOrEmpty(script))
                {
                    list.Add(new FieldForGet() { Name = "错误", Description = "未输入要执行的脚本。", Type = FieldType.MessageBox });
                    return list.ToArray();
                }
                string host = null;
                int port = 0;
                string user = null;
                string password = null;

                switch (request.GetFieldValue("tab", "ConnectionInfo", "ConnectTo"))
                {
                    case "Self":
                        var connectInfo = Config.Instance.GetConnectInfo();
                        host = connectInfo.Address.ToString();
                        port = connectInfo.Port;
                        var connectUserInfo = UserManage.Instance.GetConnectUserInfo();
                        user = connectUserInfo?.Name;
                        password = connectUserInfo?.Password;
                        break;
                    case "Other":
                        host = request.GetFieldValue("tab", "ConnectionInfo", "Host");
                        port = int.Parse(request.GetFieldValue("tab", "ConnectionInfo", "Port"));
                        user = request.GetFieldValue("tab", "ConnectionInfo", "User");
                        password = request.GetFieldValue("tab", "ConnectionInfo", "Password");
                        break;
                }
                
                var builder = Cluster.Builder()
                        .AddContactPoint(host)
                        .WithPort(port);
                if (!string.IsNullOrEmpty(user))
                    builder = builder.WithCredentials(user, password);

                try
                {
                    DbUtils.UseSession(host, port, user, password, session =>
                    {
                        var rs = session.Execute(script);
                        StringBuilder sb = new StringBuilder();

                        if (rs.Columns.Length > 0)
                        {
                            sb.AppendLine("数据");
                            var columnLine = string.Join(" | ", rs.Columns.Select(t => t.Name));
                            sb.AppendLine(string.Empty.PadRight(columnLine.Length, '-'));
                            sb.AppendLine(columnLine);
                            sb.AppendLine(string.Empty.PadRight(columnLine.Length, '-'));
                            foreach (var row in rs)
                                sb.AppendLine(string.Join(" | ", row.Select(t =>
                                {
                                    if (t is IDictionary)
                                    {
                                        var dict = (IDictionary)t;
                                        List<string> tmpList = new List<string>();
                                        foreach (var tmpKey in dict.Keys)
                                            tmpList.Add($"' {tmpKey}' : '{dict[tmpKey]}'");
                                        return "{" + string.Join(",", tmpList) + "}";
                                    }
                                    return t.ToString();
                                })));
                        }
                        else
                        {
                            sb.AppendLine("无");
                        }
                        list.Add(new FieldForGet() { Name = "查询结果", Description = sb.ToString(), Type = FieldType.Alert });
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new FieldForGet() { Name = "错误", Description = ex.Message, Type = FieldType.Alert });
                }                
            }
            return list.ToArray();
        }
    }
}
