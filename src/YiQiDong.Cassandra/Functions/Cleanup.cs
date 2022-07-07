using Quick.Fields;
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

namespace YiQiDong.Cassandra.Functions
{
    class Cleanup : AbstractFunction
    {
        public override string Name => "清理数据库";

        private string imageFolder;
        private string containerFolder;

        public Cleanup(string imageFolder, string containerFolder)
        {
            this.imageFolder = imageFolder;
            this.containerFolder = containerFolder;
        }

        public override FieldForGet[] Get()
        {
            var list = new List<FieldForGet>();
            list.Add(new FieldForGet()
            {
                Name = "说明",
                Description = "发送清理指令是调用命令行：nodetool cleanup，用于释放无效数据占用的磁盘空间。",
                Type = FieldType.Alert
            });
            list.Add(new FieldForGet() { Id = "Execute", Name = "发送清理指令", Type = FieldType.Button });
            return list.ToArray();
        }

        private void throwException_NotSupportOsAndArchitecture()
        {
            throw new NotImplementedException($"不支持的操作系统[{RuntimeInformation.OSDescription}]+平台架构[{RuntimeInformation.OSArchitecture}]。");
        }

        public override FieldForGet[] Post(FunctionRequest request)
        {
            var list = Get().ToList();
            if (request.IsFieldIdsMatch("Execute"))
            {
                try
                {
                    var var_JAVA_HOME = "";
                    var process_filename = "";
                    var process_argument_list = new List<string>();

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        switch (RuntimeInformation.OSArchitecture)
                        {
                            case Architecture.X64:
                                var_JAVA_HOME = "jre_windows_x64";
                                process_filename = Path.Combine(imageFolder, "bin", "nodetool.bat");
                                process_argument_list.Add("cleanup");
                                break;
                            default:
                                throwException_NotSupportOsAndArchitecture();
                                break;
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        switch (RuntimeInformation.OSArchitecture)
                        {
                            case Architecture.X64:
                                var_JAVA_HOME = "jre_linux_x64";
                                process_filename = "sh";
                                process_argument_list.Add(Path.Combine(imageFolder, "bin", "nodetool"));
                                process_argument_list.Add("cleanup");
                                break;
                            case Architecture.Arm64:
                                var_JAVA_HOME = "jre_linux_arm64";
                                process_filename = "sh";
                                process_argument_list.Add(Path.Combine(imageFolder, "bin", "nodetool"));
                                process_argument_list.Add("cleanup");
                                break;
                            case Architecture.Arm:
                                var_JAVA_HOME = "jre_linux_arm";
                                process_filename = "sh";
                                process_argument_list.Add(Path.Combine(imageFolder, "bin", "nodetool"));
                                process_argument_list.Add("cleanup");
                                break;
                            default:
                                throwException_NotSupportOsAndArchitecture();
                                break;
                        }
                    }
                    else
                    {
                        throwException_NotSupportOsAndArchitecture();
                    }

                    var_JAVA_HOME = Path.Combine(imageFolder, var_JAVA_HOME);
                    ProcessStartInfo psi = new ProcessStartInfo(process_filename);
                    foreach (var item in process_argument_list)
                        psi.ArgumentList.Add(item);
                    AgentContext.Instance.LogInfo("清理进程文件：" + process_filename);
                    AgentContext.Instance.LogInfo("清理进程参数：" + psi.Arguments);

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

                    var process = Process.Start(psi);
                    process.EnableRaisingEvents = true;
                    process.OutputDataReceived += Process_OutputDataReceived;
                    process.ErrorDataReceived += Process_ErrorDataReceived;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    AgentContext.Instance.LogInfo($"清理进程[Id:{process.Id},Name:{process.ProcessName}]已经启动。");
                    process.Exited += Process_Exited;

                    list.Add(new FieldForGet()
                    {
                        Name = "成功",
                        Description = $"发送清理指令成功！",
                        Type = FieldType.MessageBox
                    });
                }
                catch (Exception ex)
                {
                    list.Add(new FieldForGet()
                    {
                        Name = "错误",
                        Description = "发送清理指令失败！原因：" + ExceptionUtils.GetExceptionMessage(ex),
                        Type = FieldType.MessageBox
                    });
                }
            }
            return list.ToArray();
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            var process = sender as Process;
            if (process == null)
                return;
            AgentContext.Instance.LogInfo($"清理进程[Id:{process.Id},Name:{process.ProcessName}]已经退出，退出码：{process.ExitCode}。");
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

    }
}
