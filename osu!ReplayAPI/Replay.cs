using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ReplayAPI
{
    public class Replay : IDisposable
    {
        public GameModes GameMode;
        public string Filename;
        public int FileFormat;
        public string MapHash;
        public string PlayerName;
        public string ReplayHash;
        public uint TotalScore;
        public UInt16 Count300;
        public UInt16 Count100;
        public UInt16 Count50;
        public UInt16 CountGeki;
        public UInt16 CountKatu;
        public UInt16 CountMiss;
        public UInt16 MaxCombo;
        public bool IsPerfect;
        public Mods Mods;
        public List<LifeInfo> LifeFrames = new List<LifeInfo>();
        public DateTime PlayTime;
        public int ReplayLength;
        public List<ReplayInfo> ReplayFrames = new List<ReplayInfo>();
        public int Seed;

        private BinaryReader replayReader;
        private CultureInfo culture = new CultureInfo("en-US", false);
        private bool headerLoaded;
        private bool fullLoaded;

        public Replay()
        { }

        public Replay(string replayFile, bool fullLoad = false)
        {
            Filename = replayFile;
            replayReader = new BinaryReader(new FileStream(replayFile, FileMode.Open, FileAccess.Read, FileShare.Read));

            loadHeader();
            if (fullLoad)
                Load();
        }

        private void loadHeader()
        {
            GameMode = (GameModes)Enum.Parse(typeof(GameModes), replayReader.ReadByte().ToString(culture));
            FileFormat = replayReader.ReadInt32();
            MapHash = readString(replayReader);
            PlayerName = readString(replayReader);
            ReplayHash = readString(replayReader);
            Count300 = replayReader.ReadUInt16();
            Count100 = replayReader.ReadUInt16();
            Count50 = replayReader.ReadUInt16();
            CountGeki = replayReader.ReadUInt16();
            CountKatu = replayReader.ReadUInt16();
            CountMiss = replayReader.ReadUInt16();
            TotalScore = replayReader.ReadUInt32();
            MaxCombo = replayReader.ReadUInt16();
            IsPerfect = replayReader.ReadBoolean();
            Mods = (Mods)replayReader.ReadInt32();
            headerLoaded = true;
        }

        /// <summary>
        /// Loads Metadata if not already loaded and loads Lifedata, Timestamp, Playtime and Clicks.
        /// </summary>
        public void Load()
        {
            if (!headerLoaded)
                loadHeader();
            if (fullLoaded)
                return;

            //Life
            string lifeData = replayReader.ReadByte() == 0 ? null : replayReader.ReadString();
            if (!string.IsNullOrEmpty(lifeData))
            {
                foreach (string lifeBlock in lifeData.Split(','))
                {
                    string[] split = lifeBlock.Split('|');
                    if (split.Length < 2)
                        continue;

                    LifeFrames.Add(new LifeInfo()
                    {
                        Time = int.Parse(split[0], culture.NumberFormat),
                        Percentage = float.Parse(split[1], culture.NumberFormat)
                    });
                }
            }

            Int64 ticks = replayReader.ReadInt64();
            PlayTime = new DateTime(ticks, DateTimeKind.Utc);

            ReplayLength = replayReader.ReadInt32();

            //Data
            if (ReplayLength > 0)
            {
                int lastTime = 0;
                using (MemoryStream codedStream = LZMACoder.Decompress(replayReader.BaseStream as FileStream))
                using (StreamReader sr = new StreamReader(codedStream))
                {
                    foreach (string frame in sr.ReadToEnd().Split(','))
                    {
                        if (string.IsNullOrEmpty(frame))
                            continue;

                        string[] split = frame.Split('|');
                        if (split.Length < 4)
                            continue;

                        if (split[0] == "-12345")
                        {
                            Seed = int.Parse(split[3], culture.NumberFormat);
                            continue;
                        }

                        ReplayFrames.Add(new ReplayInfo()
                        {
                            TimeDiff = int.Parse(split[0], culture.NumberFormat),
                            Time = int.Parse(split[0], culture.NumberFormat) + lastTime,
                            X = float.Parse(split[1]),
                            Y = float.Parse(split[2]),
                            Keys = (Keys)Enum.Parse(typeof(Keys), split[3])
                        });
                        lastTime = ReplayFrames[ReplayFrames.Count - 1].Time;
                    }
                }
            }

            fullLoaded = true;
        }

        public void Save(string file)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                //Header
                bw.Write((byte)GameMode);
                bw.Write(FileFormat);
                writeString(bw, MapHash);
                writeString(bw, PlayerName);
                writeString(bw, ReplayHash);
                bw.Write(Count300);
                bw.Write(Count100);
                bw.Write(Count50);
                bw.Write(CountGeki);
                bw.Write(CountKatu);
                bw.Write(CountMiss);
                bw.Write(TotalScore);
                bw.Write((UInt16)MaxCombo);
                bw.Write(IsPerfect);
                bw.Write((int)Mods);

                //Life
                string rawLife = "";
                for (int i = 0; i < LifeFrames.Count; i++)
                    rawLife += string.Format("{0}|{1},", LifeFrames[i].Time.ToString(culture.NumberFormat), LifeFrames[i].Percentage.ToString(culture.NumberFormat));
                writeString(bw, rawLife);

                bw.Write(PlayTime.ToUniversalTime().Ticks);

                //Data
                if (ReplayFrames.Count == 0)
                    bw.Write(-1);
                else
                {
                    string rawData = "";
                    for (int i = 0; i < ReplayFrames.Count; i++)
                        rawData += string.Format("{0}|{1}|{2}|{3},", ReplayFrames[i].TimeDiff, ReplayFrames[i].X, ReplayFrames[i].Y, (int)ReplayFrames[i].Keys);
                    byte[] rawBytes = Encoding.ASCII.GetBytes(rawData);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ms.Write(rawBytes, 0, rawBytes.Length);

                        MemoryStream codedStream = LZMACoder.Compress(ms);
                        byte[] rawBytesCompressed = new byte[codedStream.Length];
                        codedStream.Read(rawBytesCompressed, 0, rawBytesCompressed.Length);

                        bw.Write(rawBytesCompressed.Length);
                        bw.Write(rawBytesCompressed);
                    }
                }
            }
        }

        private string readString(BinaryReader br)
        {
            if (br.ReadByte() == 0)
                return null;
            return br.ReadString();
        }

        private void writeString(BinaryWriter bw, string data)
        {
            if (string.IsNullOrEmpty(data))
                bw.Write(0);
            else
            {
                bw.Write((byte)0x0B);
                bw.Write(data);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool state)
        {
            if (replayReader != null)
                replayReader.Close();
            ReplayFrames.Clear();
            LifeFrames.Clear();
        }
    }
}
