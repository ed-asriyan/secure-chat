using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой тройку T1, T2 и T3
    /// <typeparam name="T1">Первый эелемент в тройке</typeparam>
    /// <typeparam name="T2">Второй элемент в тройке</typeparam>
    /// <typeparam name="T3">Третий элемент в тройке</typeparam>
    /// </summary>
    [Serializable]
    class Trio<T1, T2, T3>
    {
        public T1 t1 { get; set; }
        public T2 t2 { get; set; }
        public T3 t3 { get; set; }

        public Trio()
        {

        }
        public Trio(T1 t1)
        {
            this.t1 = t1;
        }
        public Trio(T1 t1, T2 t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }
        public Trio(T1 t1, T2 t2, T3 t3)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
        }
        public Trio(T2 t2, T3 t3)
        {
            this.t3 = t3;
            this.t2 = t2;
        }
    }
}