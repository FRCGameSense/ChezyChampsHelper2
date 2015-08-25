using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCHelper2
{
    class TBAMatch
    {
        public string comp_level { get; set; }
        public string match_number { get; set; }
        public List<TBAVideo> videos { get; set; }
        public string set_number { get; set; }
        public string key { get; set; }
        public string time { get; set; }
        public TBAScoreBreakDown score_breakdown { get; set; }
        public TBAAlliances alliances { get; set; }
        public string event_key { get; set; }
    }
}
