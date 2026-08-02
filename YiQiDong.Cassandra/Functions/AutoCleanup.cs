using Quick.Fields;
using Quick.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Cassandra.Core;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra.Functions
{
    class AutoCleanup : AbstractFunction
    {
        public override string Name => "定时清理快照";
        public override bool IsVisiable() => AgentContext.Container.AutoStart;
        private string containerFolder;
        public AutoCleanup(string containerFolder)
        {
            this.containerFolder = containerFolder;
        }

        private List<FieldForGet> innerGet(FunctionRequest request)
        {
            var list = new List<FieldForGet>();
            var config = AutoCleanupManager.Instance.Config;

            var tmpKey = nameof(Core.AutoCleanupConfig.Enable);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "是否启用",
                Type = FieldType.InputSelect,
                PostOnChanged = true,
                Value = request == null ? config.Enable.ToString() : request.GetFieldValue(tmpKey),
                InputSelect_Options = new Dictionary<string, string>()
                {
                    ["True"] = "是",
                    ["False"] = "否"
                }
            });

            tmpKey = nameof(AutoCleanupConfig.Trigger);
            list.Add(new FieldForGet()
            {
                Id = tmpKey,
                Name = "触发时间",
                Type = FieldType.InputText,
                Value = request == null ? config.Trigger.ToString() : request.GetFieldValue(tmpKey),
            });
            list.Add(new FieldForGet()
            {
                Type = FieldType.Alert,
                Name = "格式",
                Description = @"* * * * *
- - - - -
| | | | |
| | | | +--- 星期 (0 - 6) (星期天=0)
| | | +----- 月 (1 - 12)
| | +------- 日 (1 - 31)
| +--------- 时 (0 - 23)
+----------- 分 (0 - 59)",
                Html_Class = "alert-secondary"
            });
            list.Add(new FieldForGet() { Id = "Save", Name = "保存", Type = FieldType.Button });
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
            if (request.IsFieldIdsMatch("Save"))
            {
                try
                {
                    var enable = bool.Parse(request.GetFieldValue(nameof(Core.AutoCleanupConfig.Enable))); ;
                    var trigger = request.GetFieldValue(nameof(Core.AutoCleanupConfig.Trigger));

                    var crontabSchedule = NCrontab.CrontabSchedule.TryParse(trigger);
                    if (crontabSchedule == null)
                        throw new FormatException($"触发时机[{trigger}]格式不正确！");

                    var config = new AutoCleanupConfig()
                    {
                        Enable = enable,
                        Trigger = trigger
                    };
                    AutoCleanupManager.Instance.UpdateConfig(config);

                    list.Add(new FieldForGet()
                    {
                        Name = "成功",
                        Description = "保存成功！",
                        Type = FieldType.MessageBox
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new FieldForGet()
                    {
                        Name = "错误",
                        Description = "保存时出错！原因：" + ExceptionUtils.GetExceptionMessage(ex),
                        Type = FieldType.MessageBox
                    });
                }
            }
            return list.ToArray();
        }
    }
}
