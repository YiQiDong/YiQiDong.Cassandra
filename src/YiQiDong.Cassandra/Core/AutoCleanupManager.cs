using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YiQiDong.Agent;

namespace YiQiDong.Cassandra.Core
{
    public class AutoCleanupManager
    {
        public static AutoCleanupManager Instance { get; } = new AutoCleanupManager();

        private string configFile;
        public AutoCleanupConfig Config { get; private set; }
        private CancellationTokenSource cts;
        private NCrontab.CrontabSchedule crontabSchedule;
        private DateTime nextOccurrenceTime;

        public void Init()
        {
            configFile = Path.Combine(Agent.Instance.ContainerFolder, AutoCleanupConfig.CONFIG_FILE);
            if (File.Exists(configFile))
            {
                var configFileContent = File.ReadAllText(configFile);
                Config = JsonConvert.DeserializeObject<AutoCleanupConfig>(configFileContent);
            }
            else
            {
                Config = new AutoCleanupConfig();
            }
            Stop();
            if (Config.Enable)
                Start();
        }

        public void Start()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource();
            crontabSchedule = NCrontab.CrontabSchedule.Parse(Config.Trigger);
            nextOccurrenceTime = crontabSchedule.GetNextOccurrence(DateTime.Now);
            AgentContext.Instance.LogInfo($"[定时清理管理器]已启动定时清理。初次清理时间为[{nextOccurrenceTime.ToString("yyyy-MM-dd HH:mm:ss")}]");
            beginCleanup(cts.Token);
        }

        private void beginCleanup(CancellationToken token)
        {
            Task.Delay(1000, token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                if (DateTime.Now >= nextOccurrenceTime)
                {
                    //开始工作
                    AgentContext.Instance.LogInfo($"[定时清理管理器]开始清理。。。");
                    if (Agent.Instance.ContainerInfo.AutoStart)
                    {
                        Agent.Instance.RunNodeTool(AgentContext.Instance.LogInfo, "cleanup");
                        Agent.Instance.RunNodeTool(AgentContext.Instance.LogInfo, "clearsnapshot");
                    }
                    else
                    {
                        AgentContext.Instance.LogInfo("[定时清理管理器]容器当前未运行，已跳过此次清理。");
                    }
                    //设置下一次触发时间
                    nextOccurrenceTime = crontabSchedule.GetNextOccurrence(DateTime.Now);
                    AgentContext.Instance.LogInfo($"[定时清理管理器]清理完成，下次清理时间为[{nextOccurrenceTime.ToString("yyyy-MM-dd HH:mm:ss")}]");
                }
                beginCleanup(token);
            });
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts?.Cancel();
                cts = null;
                AgentContext.Instance.LogInfo("[定时清理管理器]已停止定时清理。");
            }
        }

        public void UpdateConfig(AutoCleanupConfig config)
        {
            //更新配置模型对象
            Config = config;
            //写入到文件
            File.WriteAllText(configFile, JsonConvert.SerializeObject(Config));

            if (Config.Enable)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }
    }
}
