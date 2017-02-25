using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    class IPEventArgs
    {
        public IPAddress IpAddress { get; set; }
        public IPEventArgs()
        {

        }
        public IPEventArgs(IPAddress ipAddress)
        {
            this.IpAddress = ipAddress;
        }
    }
}
