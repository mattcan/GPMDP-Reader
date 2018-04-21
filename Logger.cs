using System;

namespace gpmdp_rdr
{
    public class Logger
    {
        public Logger(bool isDebugMode = false) {
            this.DebugMode = isDebugMode;
        }

        public bool DebugMode { get; set; } = false;

        public void Debug(string message) {
            if (this.DebugMode) {
                Console.WriteLine(message);
            }
        }
    }
}