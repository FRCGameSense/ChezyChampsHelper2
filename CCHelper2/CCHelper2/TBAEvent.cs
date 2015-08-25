using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCHelper2
{
    class TBAEvent
    {
        public string key { get; set; }
        public string website { get; set; }
        public bool? official { get; set; }
        public string end_date { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public string facebook_eid { get; set; }
        public string event_district_string { get; set; }
        public string venue_address { get; set; }
        public int? event_district { get; set; }
        public string location { get; set; }
        public string event_code { get; set; }
        public int? year { get; set; }
        public List<TBAWebcast> webcast { get; set; }
        public List<TBAEventAlliance> alliances { get; set; }
        public string event_type_string { get; set; }
        public string start_date { get; set; }
        public int? event_type { get; set; }
    }
}
