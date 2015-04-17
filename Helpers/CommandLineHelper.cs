using System;
using System.Diagnostics;

namespace SVNSlackNotifier.Helpers
{
    public class CommandLineHelper
    {
        public static string ExecuteProcess(string processFullPath, string args)
        {
            var output = string.Empty;
            try
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false; // UseShellExecute must be false to redirect output
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = processFullPath;
                process.StartInfo.Arguments = args;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            catch(Exception e)
            {
                Logger.Shared.WriteError(e, "ExecuteProcess: Failed to execute process");
            }
            return output;
        }
    }
}
