using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows.Media.Imaging;

namespace Chat
{
    class IncCanvasBackgroundChanged: EventArgs
    {
        public IPAddress ipAddress { get; set; }
        public byte[] bitmap { get; set; }

        public IncCanvasBackgroundChanged()
        {

        }
        public IncCanvasBackgroundChanged(IPAddress ipAddress, byte[] bitmap)
        {
            this.ipAddress = ipAddress;
            this.bitmap = bitmap;
        }
    }
}
