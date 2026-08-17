using Quick.Fields;
using Quick.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra.Functions
{
    class CassandraTool : AbstractFunction
    {
        public override bool IsVisiable() => AgentContext.Container.AutoStart;
        private string containerFolder;

        public CassandraTool(string containerFolder)
        {
            this.containerFolder = containerFolder;
        }

        public override string Name => "Cassandra工具";

        public override List<FieldForGet> Execute(FunctionRequest request)
        {
            if (request == null)
                return Get();
            return Post(request);
        }

        public List<FieldForGet> Get()
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
                    ["nodetool repair"] = "修复",
                    ["nodetool cleanup"] = "清理",
                    ["nodetool clearsnapshot"] = "清理快照"
                }
            });
            list.Add(new FieldForGet()
            {
                Id = "Arguments",
                Name = "命令",
                Description = "Cassandra命令与参数",
                Type = FieldType.InputText
            });
            list.Add(new FieldForGet() { Id = "Execute", Name = "发送指令", Type = FieldType.Button });
            return list;
        }

        public List<FieldForGet> Post(FunctionRequest request)
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
                        throw new Exception("请输入命令与参数！");
                    var argumentsSegments = arguments.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var resultFile = System.IO.Path.Combine(containerFolder, $"执行结果_{argumentsSegments[0]}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.log");
                    StringBuilder sb = new StringBuilder();
                    AgentContext.LogInfo($"开始执行命令：{arguments}");
                    sb.AppendLine($">{arguments}");
                    bool waitResult = false;
                    var executeTask = Task.Run(() =>
                    {
                        Agent.Instance.RunNodeTool(
                        log => sb.AppendLine(log),
                        argumentsSegments);
                    });
                    waitResult = executeTask.Wait(5000);
                    executeTask.ContinueWith(t =>
                    {
                        if (waitResult)
                        {
                            AgentContext.LogInfo($"执行命令完成：{arguments}");
                        }
                        else
                        {
                            File.WriteAllText(resultFile, sb.ToString());
                            AgentContext.LogInfo($"执行命令完成：{arguments}，执行结果输出文件：{resultFile}");
                        }
                    });
                    if (waitResult)
                    {
                        list.Add(new FieldForGet()
                        {
                            Name = "执行结果",
                            Description = sb.ToString(),
                            Type = FieldType.Alert,
                            Html_Class = "alert-secondary"
                        });
                    }
                    else
                    {
                        list.Add(new FieldForGet()
                        {
                            Name = "指令执行中",
                            Description = $"指令正在执行，执行完成后结果会输出到文件：{resultFile}",
                            Type = FieldType.Alert,
                            Html_Class = "alert-secondary"
                        });
                    }
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
            return list;
        }
    }
}
