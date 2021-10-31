using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WtTools.Formats.Extensions;
using ZstdNet;

namespace WtTools.Formats
{
    public class WrlpInfo
    {
        public uint Version { get; private set; }
        public string Level { get; private set; }
        public string LevelSettings { get; private set; }
        public string BattleLayout { get; private set; }
        public string Environment { get; private set; }
        public string Visibility { get; private set; }
        public ulong Ssid { get; private set; }
        public string Location { get; private set; }
        public uint StartTime { get; private set; }
        public uint TimeLimit { get; private set; }
        public uint ScoreLimit { get; private set; }
        public string BattleType { get; private set; }
        public string BattleKillStreak { get; private set; }
        public BlkInfo MSet { get; private set; }
        public BlkInfo Rez { get; private set; }
        public byte[] Wrplu { get; private set; }

        private uint _rezOffset;
        private uint _mSetSize;


        public WrlpInfo(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var reader = new BinaryReader(ms);
            var magic = reader.ReadUInt32();
            if (magic != 0x1000ace5)
            {
                throw new ArgumentException($"Unknown MAGIC word {magic:x}");
            }
            ReadHeader(reader);
            var mSet = reader.ReadBytes((int)_mSetSize);
            var wrpluOffset = reader.BaseStream.Position;
            Wrplu = reader.ReadBytes((int)_rezOffset - (int)wrpluOffset);
            var rez = reader.ReadToEnd();
            MSet = new BlkInfo("mset.blk", mSet);
            Rez = new BlkInfo("rez.blk", rez);
        }

        private void ReadHeader(BinaryReader reader)
        {
            Version = reader.ReadUInt32();
            Level = reader.ReadBytes(128).ToUTF8String().Trim('\0');
            LevelSettings = reader.ReadBytes(260).ToUTF8String().Trim('\0');
            BattleLayout = reader.ReadBytes(128).ToUTF8String().Trim('\0');
            Environment = reader.ReadBytes(128).ToUTF8String().Trim('\0');
            Visibility = reader.ReadBytes(32).ToUTF8String().Trim('\0');
            _rezOffset = reader.ReadUInt32();
            _ = reader.ReadBytes(40);
            Ssid = reader.ReadUInt64();
            _ = reader.ReadBytes(8);
            _mSetSize = reader.ReadUInt32();
            _ = reader.ReadBytes(28);
            Location = reader.ReadBytes(128).ToUTF8String().Trim('\0');
            StartTime = reader.ReadUInt32();
            TimeLimit = reader.ReadUInt32();
            ScoreLimit = reader.ReadUInt32();
            _ = reader.ReadBytes(48);
            BattleType = reader.ReadBytes(128).ToUTF8String().Trim('\0');
            BattleKillStreak = reader.ReadBytes(128).ToUTF8String().Trim('\0');
        }
    }
}
