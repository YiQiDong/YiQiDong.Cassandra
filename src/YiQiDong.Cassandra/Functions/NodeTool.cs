using Quick.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
            if (request.IsFieldIdsMatch("Execute"))
            {
                try
                {
                    var arguments = request.GetFieldValue("Arguments");
                    AgentContext.Instance.LogInfo($"发送命令：nodetool {arguments}");
                    Agent.Instance.RunNodeTool(AgentContext.Instance.LogInfo, arguments);

                    list.Add(new FieldForGet()
                    {
                        Name = "成功",
                        Description = $"发送指令成功！",
                        Type = FieldType.MessageBox
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new FieldForGet()
                    {
                        Name = "错误",
                        Description = "发送指令失败！原因：" + ExceptionUtils.GetExceptionMessage(ex),
                        Type = FieldType.MessageBox
                    });
                }
            }
            return list.ToArray();
        }
    }
}
