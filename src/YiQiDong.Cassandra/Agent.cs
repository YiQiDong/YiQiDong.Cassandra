using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using YiQiDong.Protocol.V1.Model;

namespace YiQiDong.Cassandra
{
    public class Agent : AbstractAgent
    {
        public static Agent Instance { get; private set; }
        public Process Process { get; set; }

        public override void Init(ContainerInfo contentInfo)
        {
            Instance = this;
            base.Init(contentInfo);            

            var imageFolder = ImagePathUtils.GetImageFolder(ContainerInfo.ImageId);
            var containerFolder = ContainerPathUtils.GetContainerFolder(ContainerInfo.Id);

            AddFunction(new Functions.Config(imageFolder, containerFolder));
            AddFunction(new Functions.UserManage(imageFolder, containerFolder), true);
            AddFunction(new Functions.CqlQuery());

            var logsFolder = Path.Combine(containerFolder, "logs");
            if (!Directory.Exists(logsFolder))
                Directory.CreateDirectory(logsFolder);
        }

        public override void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    innnerStart();
                }
                catch (Exception ex)
                {
                    AgentContext.Instance.LogError($"启动容器时失败，原因：{ex}");
                }
            });
        }

        private void outputNotSupportOsAndArchitecture()
        {
            AgentContext.Instance.LogWarn($"不支持的操作系统[{RuntimeInformation.OSDescription}]+平台架构[{RuntimeInformation.OSArchitecture}]。");
        }

        private void innnerStart()
        {
            if (Process != null)
                return;
            if (!ContainerInfo.AutoStart)
                return;

            var imageFolder = ImagePathUtils.GetImageFolder(ContainerInfo.ImageId);
            var containerFolder = ContainerPathUtils.GetContainerFolder(ContainerInfo.Id);

            var var_JAVA_HOME = "";
            var process_filename = "";
            var process_arguments = "";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        var_JAVA_HOME = "jre_windows_x64";
                        process_filename = Path.Combine(imageFolder, "bin", "cassandra.bat");
                        process_arguments = null;
                        break;
                    default:
                        outputNotSupportOsAndArchitecture();
                        return;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        var_JAVA_HOME = "jre_linux_x64";
                        process_filename = "sh";
                        process_arguments = Path.Combine(imageFolder, "bin", "cassandra");
                        break;
                    case Architecture.Arm64:
                        var_JAVA_HOME = "jre_linux_arm64";
                        process_filename = "sh";
                        process_arguments = Path.Combine(imageFolder, "bin", "cassandra");
                        break;
                    case Architecture.Arm:
                        var_JAVA_HOME = "jre_linux_arm";
                        process_filename = "sh";
                        process_arguments = Path.Combine(imageFolder, "bin", "cassandra");
                        break;
                    default:
                        outputNotSupportOsAndArchitecture();
                        return;
                }
                //检测是否支持free命令
                AgentContext.Instance.LogInfo("正在检测是否支持常用Linux命令...");
                try
                {
                    Process.Start("free");
                    AgentContext.Instance.LogInfo("检测通过，当前系统支持常用Linux命令。");
                }
                catch (Exception ex)
                {
                    AgentContext.Instance.LogWarn("检测到不支持常用Linux命令，异常：" + ex.Message);

                    var srcBusyboxPath = Path.Combine(imageFolder, var_JAVA_HOME, "bin", "busybox");
                    AgentContext.Instance.LogInfo($"正在安装busybox...");
                    var desBusyboxPath = "/usr/bin/busybox";
                    File.Copy(srcBusyboxPath, desBusyboxPath, true);
                    var busyboxProcess = Process.Start(desBusyboxPath, "--install");
                    busyboxProcess.WaitForExit();
                    if (busyboxProcess.ExitCode == 0)
                        AgentContext.Instance.LogInfo("安装busybox成功。");
                    else
                        AgentContext.Instance.LogWarn("安装busybox失败，退出码：" + busyboxProcess.ExitCode);
                }
            }
            else
            {
                outputNotSupportOsAndArchitecture();
                return;
            }
            AgentContext.Instance.LogInfo("Process Filename：" + process_filename);
            AgentContext.Instance.LogInfo("Process Arguments：" + process_arguments);

            var_JAVA_HOME = Path.Combine(imageFolder, var_JAVA_HOME);
            ProcessStartInfo psi = new ProcessStartInfo(process_filename, process_arguments);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = containerFolder;
            var path = psi.EnvironmentVariables["PATH"];
            path += Path.PathSeparator + Path.Combine(var_JAVA_HOME, "bin");
            psi.EnvironmentVariables["PATH"] = path;
            psi.EnvironmentVariables["JAVA_HOME"] = var_JAVA_HOME;
            psi.EnvironmentVariables["CONTAINER_HOME"] = containerFolder;

            Process = Process.Start(psi);
            Process.EnableRaisingEvents = true;
            Process.OutputDataReceived += Process_OutputDataReceived;
            Process.ErrorDataReceived += Process_ErrorDataReceived;
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();
            AgentContext.Instance.LogInfo($"进程[Id:{Process.Id},Name:{Process.ProcessName}]已经启动。");
            Process.Exited += Process_Exited;
            RaiseEvent_FunctionListChanged();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            AgentContext.Instance.LogInfo(e.Data);
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;
            AgentContext.Instance.LogWarn(e.Data);
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
            AgentContext.Instance.LogInfo($"进程[Id:{Process.Id},Name:{Process.ProcessName}]已经退出，退出码：{Process.ExitCode}。");
            Process = null;
            delayStart();
        }

        public override void Stop()
        {
            RaiseEvent_FunctionListChanged();
            if (Process == null)
                return;
            ProcessUtils.KillProcessTree(Process);
        }
    }
}
