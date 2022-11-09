using Quick.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra.Functions
{
    class NodeTool : AbstractFunction
    {
        public override string Name => "nodetool工具";

        public override FieldForGet[] Get()
        {
            var list = new List<FieldForGet>();
            list.Add(new FieldForGet()
            {
                Id = "CommonCommands",
                Name = "常用指令",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["repair"] = "修复",
                    ["cleanup"] = "清理",
                    ["clearsnapshot"] = "清理快照"
                }
            });
            list.Add(new FieldForGet()
            {
                Id = "Arguments",
                Name = "参数",
                Description = "执行nodetool的参数部分",
                Type = FieldType.InputText
            });
            list.Add(new FieldForGet() { Id = "Execute", Name = "发送指令", Type = FieldType.Button });
            return list.ToArray();
        }

        public override FieldForGet[] Post(FunctionRequest request)
        {
            var list = Get().ToList();
            if (request.IsFieldIdsMatch("CommonCommands"))
            {
                list[1].Value = request.GetFieldValue("CommonCommands");
            }
            else if (request.IsFieldIdsMatch("Execute"))
            {
                try
                {
                    var arguments = request.GetFieldValue("Arguments");
                    if (string.IsNullOrEmpty(arguments))
                        throw new Exception("请输入参数！");
                    
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($">nodetool {arguments}");
                    Agent.Instance.RunNodeTool(
                        log => sb.AppendLine(log),
                        arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                    list.Add(new FieldForGet()
                    {
                        Name = "输出",
                        Description = sb.ToString(),
                        Type = FieldType.Alert,
                        Html_Class = "alert-secondary"
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new FieldForGet()
                    {
                        Name = "错误",
                        Description = "发送指令失败！原因：" + ExceptionUtils.GetExceptionMessage(ex),
                        Type = FieldType.Alert,
                        Html_Class = "alert-danger"
                    });
                }
            }
            return list.ToArray();
        }
    }
}
