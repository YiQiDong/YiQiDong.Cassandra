using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinNodetool.Core
{
    public class CassandraUtils
    {
        private static void throwException_NotSupportOsAndArchitecture()
        {
            throw new NotImplementedException($"不支持的操作系统[{RuntimeInformation.OSDescription}]+平台架构[{RuntimeInformation.OSArchitecture}]。");
        }

        public static void RunNodeTool(string imageFolder,string containerFolder, Action<string> pushLog, params string[] args)
        {
            if (!Directory.Exists(imageFolder))
                throw new ApplicationException($"配置的Cassandra镜像目录[{imageFolder}]不存在！");
            if (!Directory.Exists(containerFolder))
                throw new ApplicationException($"配置的Cassandra容器目录[{containerFolder}]不存在！");

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
                        break;
                    case Architecture.Arm64:
                        var_JAVA_HOME = "jre_linux_arm64";
                        process_filename = "sh";
                        process_argument_list.Add(Path.Combine(imageFolder, "bin", "nodetool"));
                        break;
                    case Architecture.Arm:
                        var_JAVA_HOME = "jre_linux_arm";
                        process_filename = "sh";
                        process_argument_list.Add(Path.Combine(imageFolder, "bin", "nodetool"));
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

            if (args != null && args.Length > 0)
                process_argument_list.AddRange(args);

            var_JAVA_HOME = Path.Combine(imageFolder, var_JAVA_HOME);
            ProcessStartInfo psi = new ProcessStartInfo(process_filename);
            
            foreach (var item in process_argument_list)
                psi.ArgumentList.Add(item);

            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = containerFolder;
            var path = psi.EnvironmentVariables["PATH"];
            path += Path.PathSeparator + Path.Combine(var_JAVA_HOME, "bin");
            psi.EnvironmentVariables["PATH"] = path;
            psi.EnvironmentVariables["JAVA_HOME"] = var_JAVA_HOME;
            psi.EnvironmentVariables["CONTAINER_HOME"] = containerFolder;

            var process = Process.Start(psi);
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, e) => pushLog(e.Data);
            process.ErrorDataReceived += (sender, e) => pushLog(e.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
