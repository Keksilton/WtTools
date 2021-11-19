using System.Collections.Generic;

namespace WtTools.Web.Data.Wrpl
{
    public class Player
    {
        public string Name { get; set; }
        public float Elo { get; set; }
        public int Team { get; set; }
        public string ClanTag { get; set; }
        public string Platform { get; set; }
        public int Id { get; set; }
        public int Slot { get; set; }
        public string Country { get; set; }
        public int Mrank { get; set; }
        public bool Auto_squad { get; set; }
        public int Wait_time { get; set; }
        public int ClanId { get; set; }
        public int Tier { get; set; }
        public int Squad { get; set; }
        public int Rank { get; set; }
        public Dictionary<string, string> Crafts { get; set; }
        public CraftsInfoArray Crafts_info { get; set; }
    }
}
