using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;

namespace Chat
{
    class RecievedMessageEventArgs
    {
        public Message Message { get; set; }
        public IPAddress Sender { get; set; }
        public RecievedMessageEventArgs()
        {

        }
        public RecievedMessageEventArgs(Message message, IPAddress sender)
        {
            this.Message = message;
            this.Sender = sender;
        }
    }
}
