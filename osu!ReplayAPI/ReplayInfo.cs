using System;

namespace ReplayAPI
{
    public class ReplayInfo
    {
        public Int64 TimeDiff;
        public int Time;
        [System.ComponentModel.DisplayName("Time In Seconds")]
        public float TimeInSeconds { get { return Time/1000f; } }
        public float X { get; set; }
        public float Y { get; set; }
        public KeyData Keys { get; set; }
    }
}
