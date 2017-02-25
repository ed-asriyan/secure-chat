using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


namespace Chat
{
    class IncCanvasSizeChanged
    {
        public IPAddress ipAddress { get; set; }
        public double X { get; set; }
        public double Y { set; get; }
        public IncCanvasSizeChanged()
        {

        }
        public IncCanvasSizeChanged(IPAddress ipAddress, double x, double y)
        {
            this.ipAddress = ipAddress;
            this.X = x;
            this.Y = y;
        }
    }
}
