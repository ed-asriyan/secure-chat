using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class ProcessEventArgs: EventArgs
    {
        public long Max { get; set; }
        public long Min { get; set; }
        public long Value { get; set; }
        public ProcessEventArgs()
        {

        }
        public ProcessEventArgs(long Min, long Max, long Value)
        {
            this.Min = Min;
            this.Max = Max;
            this.Value = Value;
        }
        
    }
}
