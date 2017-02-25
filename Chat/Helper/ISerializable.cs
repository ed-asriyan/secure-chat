using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Интерфейс, способный сериализоваться
    /// </summary>
    interface ISerializable
    {
        byte[] ConventToBytes();
        void ConventFromBytes(byte[] bytes, int startIndex);
    }
}
