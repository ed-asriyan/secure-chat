using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой пару T1 и T2
    /// <typeparam name="T1">Первый эелемент в паре</typeparam>
    /// <typeparam name="T2">Второй элемент в паре</typeparam>
    /// </summary>
    [Serializable]
    class Pair<T1, T2>
    {
        public T1 t1 { get; set; }
        public T2 t2 { get; set; }

        public Pair()
        {
        }
        public Pair(T1 t1)
        {
            this.t1 = t1;
        }
        public Pair(T2 t2)
        {
            this.t2 = t2;
        }
        public Pair(T1 t1, T2 t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }

    }
}