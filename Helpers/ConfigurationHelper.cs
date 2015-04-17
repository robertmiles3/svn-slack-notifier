using System;
using System.Configuration;

namespace SVNSlackNotifier.Helpers
{
    public class ConfigurationHelper
    {
        public static string SlackWebhookURL
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["SlackWebhookURL"];
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        public static string SVNLookProcessPath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["svnlookPath"];
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
