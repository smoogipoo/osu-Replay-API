using System;

namespace ReplayAPI
{
    public class ReplayInfo
    {
        public Int64 TimeDiff;
        public int Time;
        [System.ComponentModel.DisplayName("Time In Seconds")]
        public double TimeInSeconds { get { return Time/1000.0; } }
        public double X { get; set; }
        public double Y { get; set; }
        public KeyData Keys { get; set; }
    }
}
