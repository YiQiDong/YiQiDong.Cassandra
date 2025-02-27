using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Cassandra.Utils;
using YiQiDong.Core;
using YiQiDong.Core.Utils;

namespace YiQiDong.Cassandra
{
    public class Agent : AbstractAgent
    {
        public static Agent Instance { get; private set; }
        public Process Process { get; set; }

        private string imageFolder;
        public string ContainerFolder { get; private set; }

        public override void Init()
        {
            Instance = this;
            base.Init();
            if (AgentContext.IsContainerRuning)
            {
                imageFolder = AgentContext.Container.ImageFolder;
                ContainerFolder = AgentContext.Container.ContainerFolder;

                AddFunction(new Functions.Config(imageFolder, ContainerFolder));
                AddFunction(new Functions.UserManage(imageFolder), true);
                AddFunction(new Functions.CqlQuery());
                AddFunction(new Functions.CassandraTool(ContainerFolder), true);
                AddFunction(new Functions.AutoCleanup(ContainerFolder), true);
            }
        }

        public override void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    innnerStart();
                    Core.AutoCleanupManager.Instance.Init();
                }
                catch (Exception ex)
                {
                    AgentContext.LogError($"启动容器时失败，原因：{ex}");
                }
            });
        }

        private void outputNotSupportOsAndArchitecture()
        {
            AgentContext.LogWarn($"不支持的平台[{RuntimeUtils.GetCurrentRID()}]。");
        }

        private void innnerStart()
        {
            if (Process != null)
                return;
            if (!AgentContext.Container.AutoStart)
                return;

            var dataFolder = Functions.Config.Instance.GetDataFolder();

            var process_filename = "";
            var process_argument_list = new List<string>();
            var environmentVariables = new Dictionary<string, string>();

            if (OperatingSystem.IsWindows())
            {
                process_filename = Path.Combine(imageFolder, "bin", "cassandra.bat");
            }
            else if (OperatingSystem.IsMacOS())
            {
                process_filename = "sh";
                process_argument_list.Add(Path.Combine(imageFolder, "bin", "cassandra"));
                environmentVariables["LANG"] = "C";
            }
            else if (OperatingSystem.IsLinux())
            {
                process_filename = "sh";
                process_argument_list.Add(Path.Combine(imageFolder, "bin", "cassandra"));
                environmentVariables["LANG"] = "C";

                //检测是否支持free命令
                AgentContext.LogInfo("正在检测是否支持常用Linux命令...");
                try
                {
                    var psi = new ProcessStartInfo("free");
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.RedirectStandardInput = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                    AgentContext.LogInfo("检测通过，当前系统支持常用Linux命令。");
                }
                catch (Exception ex)
                {
                    AgentContext.LogError("检测到不支持常用Linux命令，异常：" + ex.Message);
                    return;
                }
            }
            else
            {
                outputNotSupportOsAndArchitecture();
                return;
            }
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(process_filename);
                foreach (var item in process_argument_list)
                    psi.ArgumentList.Add(item);
                AgentContext.LogInfo("工作进程文件：" + process_filename);
                if (psi.ArgumentList.Count > 0)
                    AgentContext.LogInfo("工作进程参数：" + string.Join(" ", psi.ArgumentList));
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.UseShellExecute = false;
                psi.WorkingDirectory = dataFolder;
                psi.EnvironmentVariables["CONTAINER_HOME"] = dataFolder;
                if (environmentVariables.Count > 0)
                    foreach (var item in environmentVariables)
                        psi.EnvironmentVariables[item.Key] = item.Value;
                AgentContext.LogInfo("正在启动工作进程...");
                Process = Process.Start(psi);
                Process.EnableRaisingEvents = true;
                Process.OutputDataReceived += Process_OutputDataReceived;
                Process.ErrorDataReceived += Process_ErrorDataReceived;
                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                AgentContext.LogInfo($"工作进程[Id:{Process.Id},Name:{Process.ProcessName}]已经启动。");
                Process.Exited += Process_Exited;
                RaiseEvent_FunctionListChanged();
            }
            catch (Exception ex)
            {
                AgentContext.LogError("启动工作进程时出错，原因：" + ExceptionUtils.GetExceptionString(ex));
            }
        }

        public void RunNodeTool(Action<string> pushLog, params string[] commandAndArgs)
        {
            var process_filename = "";
            var process_argument_list = new List<string>();
            var command = commandAndArgs[0];
            var args = commandAndArgs.Skip(1).ToArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        process_filename = Path.Combine(imageFolder, "bin", $"{command}.bat");
                        break;
                    default:
                        outputNotSupportOsAndArchitecture();
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                process_filename = "sh";
                process_argument_list.Add(Path.Combine(imageFolder, "bin", command));
            }
            else
            {
                outputNotSupportOsAndArchitecture();
            }

            if (args != null && args.Length > 0)
                process_argument_list.AddRange(args);

            ProcessStartInfo psi = new ProcessStartInfo(process_filename);
            foreach (var item in process_argument_list)
                psi.ArgumentList.Add(item);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = ContainerFolder;
            //var path = psi.EnvironmentVariables["PATH"];
            //path += Path.PathSeparator + Path.Combine(var_JAVA_HOME, "bin");
            //psi.EnvironmentVariables["PATH"] = path;
            //psi.EnvironmentVariables["JAVA_HOME"] = var_JAVA_HOME;
            //psi.EnvironmentVariables["CONTAINER_HOME"] = ContainerFolder;

            var process = Process.Start(psi);
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, e) => pushLog(e.Data);
            process.ErrorDataReceived += (sender, e) => pushLog(e.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            AgentContext.LogInfo(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            AgentContext.LogWarn(e.Data);
        }

        private void delayStart()
        {
            Task.Delay(5000).ContinueWith(t =>
            {
                innnerStart();
            });
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            AgentContext.LogInfo($"工作进程[Id:{Process.Id},Name:{Process.ProcessName}]已经退出，退出码：{Process.ExitCode}。");
            Process.OutputDataReceived -= Process_OutputDataReceived;
            Process.ErrorDataReceived -= Process_ErrorDataReceived;
            Process.Exited -= Process_Exited;
            Process = null;
            delayStart();
        }

        public override void Stop()
        {
            Core.AutoCleanupManager.Instance.Stop();
            RaiseEvent_FunctionListChanged();
            if (Process == null)
                return;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WinUtils.StopProgram(Process);
            else
                Process.Start("kill", Process.Id.ToString());
        }
    }
}
