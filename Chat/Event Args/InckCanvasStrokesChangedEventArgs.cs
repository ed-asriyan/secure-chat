using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Input;
using System.Windows.Ink;


namespace Chat
{
    class InckCanvasChangedEventArgs: EventArgs
    {
        public IPAddress ipAddress { get; set; }
        public StrokeCollection inckCanvasBytes { get; set; }
        public byte type { get; private set; }

        public InckCanvasChangedEventArgs()
        {
        }

        public InckCanvasChangedEventArgs(IPAddress ipAddress, StrokeCollection bytes)
        {
            this.ipAddress = ipAddress;
            this.inckCanvasBytes = bytes;
        }
    }
}
