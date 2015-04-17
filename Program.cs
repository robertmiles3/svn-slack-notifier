using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SVNSlackNotifier.Models;

namespace SVNSlackNotifier
{
    static class Program
    {
        private const string FLAG_PATH = "-path=";
        private const string FLAG_REVISION = "-rev=";
        private const string FLAG_NAME = "-name=";
        private const string FLAG_URL = "-url=";
        private const string FLAG_CHANNEL = "-channel=";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 0)
            {
                var argList = args.ToList();
                var notification = new Notification();
                foreach(var arg in argList)
                {
                    if (arg.ToLower().StartsWith(FLAG_PATH) && arg.Length > FLAG_PATH.Length)
                        notification.RepositoryPath = arg.Substring(FLAG_PATH.Length);
                    else if (arg.ToLower().StartsWith(FLAG_REVISION) && arg.Length > FLAG_REVISION.Length)
                        notification.Revision = arg.Substring(FLAG_REVISION.Length);
                    else if (arg.ToLower().StartsWith(FLAG_NAME) && arg.Length > FLAG_NAME.Length)
                        notification.RepositoryName = arg.Substring(FLAG_NAME.Length);
                    else if (arg.ToLower().StartsWith(FLAG_URL) && arg.Length > FLAG_URL.Length)
                        notification.RepositoryURL = arg.Substring(FLAG_URL.Length);
                    else if (arg.ToLower().StartsWith(FLAG_CHANNEL) && arg.Length > FLAG_CHANNEL.Length)
                        notification.Channel = arg.Substring(FLAG_CHANNEL.Length);
                }

                Task.Run(() => new SlackNotifier().PostNotificationAsync(notification)).Wait();
            }
        }
    }
}
