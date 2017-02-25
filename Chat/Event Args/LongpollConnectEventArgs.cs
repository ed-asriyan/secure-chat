using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    class LongpollConnectEventArgs
    {
        public IPAddress IpAddress { get; private set; }
        public int ConnectionsCount { get; private set; }

        public LongpollConnectEventArgs() { }
        public LongpollConnectEventArgs(IPAddress ipAddress, int connectionsCount)
        {
            if (connectionsCount < 0) throw new ArgumentOutOfRangeException("connectionsCount");

            this.IpAddress = ipAddress;
            this.ConnectionsCount = connectionsCount;
        }
        public LongpollConnectEventArgs(string ipAddress, int connectionsCount)
            : this(IPAddress.Parse(ipAddress), connectionsCount)
        {

        }
    }
}
