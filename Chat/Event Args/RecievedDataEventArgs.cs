using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Chat
{
    class RecievedDataEventArgs: EventArgs
    {
        public IPAddress IpAdress { get; set; }
        public byte[] Data { get; set; }
        public RecievedDataEventArgs()
        {

        }
        public RecievedDataEventArgs(IPAddress ipAdress, Stream data)
        {
            this.IpAdress = ipAdress;
            this.Data = new byte[data.Length];
            data.Position = 0;
            data.Read(this.Data, 0, this.Data.Length);
        }

        public RecievedDataEventArgs(IPAddress ipAdress, byte[] data)
        {
            this.IpAdress = ipAdress;
            this.Data = data;
        }

    }
}
