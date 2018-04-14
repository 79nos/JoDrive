using System;

namespace JoDrive.Utilities
{
    public class LogArgs : EventArgs
    {
        public string Log { get; private set; }
        public LogTypes Type { get; private set; }

        public LogArgs(string log, LogTypes type)
        {
            Log = log;
            Type = type;
        }
    }
}
