using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCHelper2
{
    class TBATeamRecord
    {
        public int? wins;
        public int? losses;
        public int? ties;

        public TBATeamRecord()
        {
            wins = 0;
            losses = 0;
            ties = 0;
        }

        public override string ToString()
        {
            return wins + "-" + losses + "-" + ties;
        }
    }
}
