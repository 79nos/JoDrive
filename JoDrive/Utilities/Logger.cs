using System;

namespace JoDrive.Utilities
{
    public enum LogTypes { Message, Warning, Error }
    public class Logger
    {
        public event EventHandler<LogArgs> OnLog;
        public event EventHandler<LogArgs> DebugLog;

        public Logger()
        {
        }
        public void Log(string log, LogTypes type)
        {
            if (type == LogTypes.Message)
                Message(log);
            else if (type == LogTypes.Warning)
                Warning(log);
            else if (type == LogTypes.Error)
                Error(log);
        }
        public void Message(string message)
        {
            string str = $"[M] {DateTime.Now.ToString("HH:mm:ss")} : {message}";
            OnLog?.Invoke(this, new LogArgs(str, LogTypes.Message));
            DebugLog?.Invoke(this, new LogArgs(str, LogTypes.Message));

        }
        public void Warning(string warning)
        {
            string str = $"[W] {DateTime.Now.ToString("HH:mm:ss")} : {warning}";
            OnLog?.Invoke(this, new LogArgs(str, LogTypes.Warning));
            DebugLog?.Invoke(this, new LogArgs(str, LogTypes.Message));

        }
        public void Error(string error)
        {
            string str = $"[E] {DateTime.Now.ToString("HH:mm:ss")} : {error}";
            OnLog?.Invoke(this, new LogArgs(str, LogTypes.Error));
            DebugLog?.Invoke(this, new LogArgs(str, LogTypes.Message));
        }
        public void Debug(string log)
        {
            string str = $"[Debug] {DateTime.Now.ToString("HH:mm:ss")} : {log}";
            DebugLog?.Invoke(this, new LogArgs(str, LogTypes.Message));
        }
    }
}
