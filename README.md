2020년 3월 이후로 TLS 1.2 미만을 허용하지 않도록 Slack 정책이 변경되었습니다.
그로 인해 정상 발신 되지 않고 다음과 같은 오류가 발생했습니다.

----------------------------------------
2021-04-22 09:58:12    [ERROR]    
이 요청을 보내는 동안 오류가 발생했습니다.
요청이 중단되었습니다. 
SSL/TLS 보안 채널을 만들 수 없습니다.
----------------------------------------

TLS 1.2 버전을 사용하도록 명시하는 코드 한 줄만 추가하였습니다.
정상 작동됨을 확인하였습니다.

2021.04.22 hj-rich

 
 
 # SVNSlackNotifier

SVNSlackNotifier is a lightweight, dependency-free .NET console app to send VisualSVN Server commit notifications to Slack.

<img src="https://s3.amazonaws.com/scs-public/svn-slack-notification.png" alt="VisualSVN Slack Notification" width="306" height="86">

## Requirements

- .NET Framework 4.5+ (uses `HttpClient`)
- If using the `-url` parameter, it currently expects the new HTML5 browser in VisualSVN Server 3.2.0+

## Installation

1. If you haven't already, add a new "Incoming WebHooks" integration in your Slack account.
2. Copy the `.exe` and `.config` files from the latest Release into a folder of your choosing that is accessible from your svn server.
3. Edit the `.config` file and replace the appSettings key `SlackWebhookURL` with the Webhook URL found in your Slack integration setup.

## Usage (VisualSVN Server)

To let SVNSlackNotifier know about commits, we need to edit the `post-commit` hook in your svn repo. For each repository you'd like notifying to Slack...

1. Open VisualSVN Server Manager and right-click on the repository. Click Properties in the context menu.
2. On the "Hooks" tab, highlight the "Post-commit hook" option and click "Edit".
3. In the field, enter the path to your `SVNSlackNotifier.exe` executable along with any parameters (see below) you would like. Parameters `-path` and `-rev` are required. The more parameters you provide, the better the output into Slack. An example might look like:
````console
"C:\SVNSlackNotifier\SVNSlackNotifier.exe" -path=%1 -rev=%2 -name=TestRepo -url=http://scswinvm:8080/svn/TestRepo
````

## Parameters

- `-path` (required) - The physical path to the repository. This is provided by the `post-commit` hook as `%1`.
- `-rev` (required) - The revision number of the commit. This is provided by the `post-commit` hook as `%2`.
- `-name` (optional) - The name of the repository. If provided, the repo name will appear in the Slack notification.
- `-url` (optional) - The URL of the repository. If provided, the revision number will be linked to the commit page.
- `-channel` (optional) - The Slack channel in which to post the notification. If provided, this will override the choice in your Slack integration setup and post to this channel instead. Format must be `#channelname` (for channel) or `@username` (for a DM).

## Troubleshooting

- A log file `SVNSlackNotifier.log` will be written to the executable's folder if any errors occur. Check there first to see what's up.
- Ensure you have replaced the app setting `SlackWebhookURL` in the `.config` file with your actual URL given by Slack when you setup your integration.
- Ensure that the `.config` file's app setting `svnlookPath` is correct for your installation. The app uses `svnlook` to lookup more details about the commit such as the author name and commit message.

## Thanks

- Thanks to the guys at VisualSVN for permission to use their logo for this little integration.

---

## [Changelog](CHANGELOG.md)

## [License](LICENSE.md)
