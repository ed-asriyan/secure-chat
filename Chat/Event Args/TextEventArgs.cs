using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    class TextEventArgs: EventArgs
    {
        public string text { get; set; }
        public IPAddress ipAddress { get; set; }
        public TextEventArgs()
        {

        }
        public TextEventArgs(string text)
        {
            this.text = text;
        }
        public TextEventArgs(IPAddress ipAddress)
        {
            this.ipAddress = ipAddress;
        }
        public TextEventArgs(IPAddress ipAddress, string text)
        {
            this.ipAddress = ipAddress;
            this.text = text;
        }
    }
}
