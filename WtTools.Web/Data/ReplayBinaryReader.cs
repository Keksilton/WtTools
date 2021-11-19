using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using WtTools.Web.Data.Wrpl;

namespace WtTools.Web.Data
{
    public class ReplayBinaryReader
    {
        public static bool VerifyMagic(byte[] data) => WtTools.Formats.WrplInfo.VerifyMagic(data);
        public WtTools.Formats.WrplInfo Wrpl { get; private set; }
        public ReplayInfo Info { get; set; }
        public ReplaySettings Settings { get; set; }
        public List<PlayerStat> Friendlies = new();
        public List<PlayerStat> Enemies = new();

        public ReplayBinaryReader(byte[] data)
        {
            Wrpl = new WtTools.Formats.WrplInfo(data);
            var dict = Wrpl.Rez.ToDictionary();
            var fullJson = JsonConvert.SerializeObject(dict);
            Info = JsonConvert.DeserializeObject<ReplayInfo>(fullJson);
            dict = Wrpl.MSet.ToDictionary();
            fullJson = JsonConvert.SerializeObject(dict);
            Console.WriteLine(Wrpl.Ssid);
            Settings = JsonConvert.DeserializeObject<ReplaySettings>(fullJson);
            var author = Info.Player.First(p => p.UserId == Info.AuthorUserId);
            Info.Player = Info.Player.OrderByDescending(p => p.Score).ToArray();
            for (int i = 0; i < Info.Player.Length; i++)
            {
                if (Info.Player[i].Team == author.Team)
                {
                    Friendlies.Add(Info.Player[i]);
                }
                else
                {
                    Enemies.Add(Info.Player[i]);
                }
            }

        }
    }
}
