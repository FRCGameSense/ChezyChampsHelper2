using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCHelper2
{
    class GSBotCommand
    {
        public string Command { get { return _cmd; } }
        public string Description { get { return _desc; } }

        private string _cmd;
        private string _desc;

        public GSBotCommand(string cmd, string desc)
        {
            this._cmd = cmd;
            this._desc = desc;
        }
    }
}
