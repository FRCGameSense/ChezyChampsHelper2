using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CCHelper2
{
    class TBATeam
    {
        public string website { get; set; }
        public string name { get; set; }
        public string locality { get; set; }
        public string region { get; set; }
        public string country { get; set; }
        public string location { get; set; }
        public int? team_number { get; set; }
        public string key { get; set; }
        public string nickname { get; set; }
        public int? rookie_year { get; set; }

        public TBATeam() { }
    }
}
