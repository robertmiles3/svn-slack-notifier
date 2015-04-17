using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SVNSlackNotifier.Helpers
{
    public sealed class Logger
    {
        private static volatile Logger instance;
        private static object syncRoot = new Object();
        private bool _HasErrors = false;
        private StringBuilder _SessionLog = new StringBuilder();
        private static string LogFilePath;
        private TextWriter LogWriter;
        public delegate void FormLoggerDelegate(string message);
        public FormLoggerDelegate LogDelegate { get; set; }

        public static Logger Shared
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Logger();
                    }
                }

                return instance;
            }
        }

        public Logger()
        {
            LogFilePath = Path.Combine(Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath), "SVNSlackNotifier.log");
            LogWriter = TextWriter.Synchronized(File.AppendText(LogFilePath));
        }

        public bool HasErrors
        {
            get { return _HasErrors; }
        }

        public string SessionLogHTML
        {
            get { return _SessionLog.ToString(); }
        }

        public void WriteMessage(string message)
        {
            try
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (!string.IsNullOrEmpty(message))
                {
                    LogWriter.WriteLine(now + "    [INFO]     " + message);
                    LogWriter.Flush();

                    _SessionLog.AppendLine("<strong style='color:#0000dd; font-size:12px;'>" + now + "</strong>     " + message + "<br />");

                    if (LogDelegate != null)
                        LogDelegate(message);
                }
            }
            catch { }
        }

        public void WriteError(string message)
        {
            if (!string.IsNullOrEmpty(message))
                WriteError(null, message);
        }

        public void WriteError(Exception e, string customMessage = "")
        {
            try
            {
                _HasErrors = true;
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (!string.IsNullOrEmpty(customMessage))
                {
                    LogWriter.WriteLine(now + "    [ERROR]    " + customMessage);
                    LogWriter.Flush();

                    _SessionLog.AppendLine("<strong style='color:#ff0000; font-size:12px;'>" + now + "</strong>     " + customMessage + "<br />");
                }
                if (e != null)
                {
                    LogWriter.WriteLine(now + "    [ERROR]    " + e.Message + (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message) ? e.InnerException.Message : ""));
                    LogWriter.Flush();
                    _SessionLog.AppendLine("<strong style='color:#ff0000; font-size:12px;'>" + now + "</strong>     " + e.Message + "<br />");
                    if (e.InnerException != null && !string.IsNullOrEmpty(e.InnerException.Message))
                        _SessionLog.AppendLine("<strong style='color:#ff0000; font-size:12px;'>" + now + "</strong>     " + e.InnerException.Message + "<br />");
                }

                // Write to UI log if available
                if (LogDelegate != null)
                {
                    if (!string.IsNullOrEmpty(customMessage))
                        LogDelegate(customMessage);
                    if (e != null)
                        LogDelegate("ERROR: " + e.Message + "\nStack Trace => " + e.StackTrace);
                }
            }
            catch { }
        }

        public void WriteDebug(string message)
        {
            try
            {
                var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                if (!string.IsNullOrEmpty(message))
                {
                    LogWriter.WriteLine(now + "    [DEBUG] " + message);
                    LogWriter.Flush();

                    // DON'T write to _SessionLog so it doesn't show up in emails

                    if (LogDelegate != null)
                        LogDelegate(message);
                }
            }
            catch { }
        }
    }
}
