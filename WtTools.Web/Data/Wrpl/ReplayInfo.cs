using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WtTools.Web.Data.Wrpl
{

    public class ReplayInfo
    {
        public string Status { get; set; }
        public float TimePlayed { get; set; }
        public string AuthorUserId { get; set; }
        public string Author { get; set; }
        public PlayerStat[] Player { get; set; }
        public UiScriptsData UiScriptsData { get; set; }
    }
}
