using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YiQiDong.Cassandra.Utils
{
    public class WinUtils
    {

        delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        public static void StopProgram(Process proc)
        {
            if (!FreeConsole())
            {
                Console.WriteLine("Failed to FreeConsole to attach to running cassandra process.  Aborting.");
                return;
            }

            if (AttachConsole((uint)proc.Id))
            {
                //Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

                // Must wait here. If we don't and re-enable Ctrl-C
                // handling below too fast, we might terminate ourselves.
                proc.StandardInput.WriteLine("Y");
                bool exited = proc.WaitForExit(30000);
                if (!exited)
                    proc.Kill();

                FreeConsole();

                // Re-enable Ctrl-C handling or any subsequently started
                // programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);

                if (exited)
                    Console.WriteLine("Successfully sent ctrl+c to process with id: " + proc.Id + ".");
                else
                    Console.WriteLine("Process with id: " + proc.Id + " did not exit after 30 seconds, killed.");
            }
            else
            {
                string errorMsg = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine("Error attaching to pid: " + proc.Id + ": " + Marshal.GetLastWin32Error() + " - " + errorMsg);
            }
        }
    }
}
