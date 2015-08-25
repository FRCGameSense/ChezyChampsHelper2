using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCHelper2
{
    class TBAEventAward
    {
        public string event_key { get; set; }
        public int? award_type { get; set; }
        public string name { get; set; }
        public List<TBAAwardRecipient> recipient_list { get; set; }
        public int? year { get; set; }
    }
}
