using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SVNSlackNotifier.Helpers;
using SVNSlackNotifier.Models;

namespace SVNSlackNotifier
{
    public class SlackNotifier
    {
        public event EventHandler OnFinished = null;

        public async Task<bool> PostNotificationAsync(Notification notification)
        {
            var isSuccess = false;

            try
            {
                if (string.IsNullOrEmpty(ConfigurationHelper.SlackWebhookURL))
                    Logger.Shared.WriteError("Missing Slack Webhook URL");
                else if(ConfigurationHelper.SlackWebhookURL == "https://hooks.slack.com/services/foo/bar/baz")
                    Logger.Shared.WriteError("Found default Slack Webhook URL in config file. Ensure you've replaced it with your own.");
                else if (string.IsNullOrEmpty(notification.RepositoryPath))
                    Logger.Shared.WriteError("Missing repo path");
                else if (string.IsNullOrEmpty(notification.Revision))
                    Logger.Shared.WriteError("Missing revision number");
                else
                {
                    // Post to Slack
                    using (var client = new HttpClient())
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, ConfigurationHelper.SlackWebhookURL);
                        var keyValues = new List<KeyValuePair<string, string>>();                        
                        var payload = BuildPayload(notification);
                        keyValues.Add(new KeyValuePair<string, string>("payload", payload));
                        request.Content = new FormUrlEncodedContent(keyValues);
                        using (var response = await client.SendAsync(request))
                        using (var content = response.Content)
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                var result = await content.ReadAsStringAsync();
                                Logger.Shared.WriteError("Failed to send notification: " + response.StatusCode + " => " + result);
                                if (result == "Payload was not valid JSON")
                                    Logger.Shared.WriteError("payload = " + payload);
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Shared.WriteError(e);
            }

            if (OnFinished != null)
                OnFinished(this, null);

            return isSuccess;
        }

        private string BuildPayload(Notification notification)
        {
            // Get some more data about this commit via "svnlook"
            notification.CommitMessage = CommandLineHelper.ExecuteProcess(ConfigurationHelper.SVNLookProcessPath, string.Format("log -r {0} {1}", notification.Revision, notification.RepositoryPath));
            notification.CommitAuthor = CommandLineHelper.ExecuteProcess(ConfigurationHelper.SVNLookProcessPath, string.Format("author -r {0} {1}", notification.Revision, notification.RepositoryPath));

            // Ensure valid formatting of message
            if(notification.CommitMessage.Contains("\""))
                notification.CommitMessage = notification.CommitMessage.Replace("\"", "\\\"");
            if (notification.CommitAuthor.Contains("\""))
                notification.CommitAuthor = notification.CommitAuthor.Replace("\"", "\\\"");
            // Trim off unnecessary trailing CRLFs
            notification.CommitMessage = notification.CommitMessage.TrimEnd(new char[] { '\r', '\n' });
            notification.CommitAuthor = notification.CommitAuthor.TrimEnd(new char[] { '\r', '\n' });

            // Use advanced message formatting for incoming webhooks
            var payloadBody = new StringBuilder();
            payloadBody.Append("{");    // begin payload
            payloadBody.Append(" \"username\" : \"VisualSVN Server\", ");
            payloadBody.Append(" \"icon_url\" : \"http://s3.amazonaws.com/scs-public/visualsvn_96.png\", ");
            payloadBody.Append(" \"attachments\" : [ { ");  // begin attachments            
            if (!string.IsNullOrEmpty(notification.Channel))
                payloadBody.Append(string.Format(" \"channel\" : \"{0}\", ", notification.Channel));
            if (!string.IsNullOrEmpty(notification.RepositoryName))
            {
                payloadBody.Append(string.Format(" \"fallback\" : \"[{0}] New commit by {1}: r{2}: {3}\", ", notification.RepositoryName, notification.CommitAuthor, notification.Revision, notification.CommitMessage));
                payloadBody.Append(string.Format(" \"pretext\" : \"[{0}] New commit by {1}\", ", notification.RepositoryName, notification.CommitAuthor));
            }
            else
            {
                payloadBody.Append(string.Format(" \"fallback\" : \"New commit by {0}: r{1}: {2}\", ", notification.CommitAuthor, notification.Revision, notification.CommitMessage));
                payloadBody.Append(string.Format(" \"pretext\" : \"New commit by {0}\", ", notification.CommitAuthor));
            }
            if (!string.IsNullOrEmpty(notification.RepositoryURL))
            {
                if (notification.RepositoryURL.Contains("/svn/"))
                    notification.RepositoryURL = notification.RepositoryURL.Replace("/svn/", "/!/#");
                notification.RepositoryURL += "/commit/r" + notification.Revision;
                payloadBody.Append(string.Format(" \"text\" : \"<{0}|r{1}>: {2}\", ", notification.RepositoryURL, notification.Revision, notification.CommitMessage));
            }
            else
                payloadBody.Append(string.Format(" \"text\" : \"r{0}: {1}\", ", notification.Revision, notification.CommitMessage));
            payloadBody.Append(" \"color\" : \"#3886C0\" ");
            payloadBody.Append("} ]"); // end attachments
            payloadBody.Append("}"); // end payload

            return payloadBody.ToString();
        }
    }
}
