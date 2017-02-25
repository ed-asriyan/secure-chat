using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace Chat
{
    class UserBeginTypingEventArgs
    {
        public IPAddress IpAddress { get; set; }
        public UserBeginTypingEventArgs()
        {
            this.IpAddress = null;
        }
        public UserBeginTypingEventArgs(IPAddress ipAdress)
        {
            this.IpAddress = ipAdress;
        }
    }
}
