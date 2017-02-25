using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой связку из четырёх элементов T1, T2, T3 и T4
    /// <typeparam name="T1">Первый эелемент в связке</typeparam>
    /// <typeparam name="T2">Второй элемент в связке</typeparam>
    /// <typeparam name="T3">Третий элемент в связке</typeparam>
    /// <typeparam name="T4">Четвёртый элемент в связке</typeparam>
    /// </summary>
    [Serializable]
    class Quadruplet<T1, T2, T3, T4>
    {
        public T1 t1 { get; set; }
        public T2 t2 { get; set; }
        public T3 t3 { get; set; }
        public T4 t4 { get; set; }

        public Quadruplet()
        {

        }
        public Quadruplet(T1 t1)
        {
            this.t1 = t1;
        }
        public Quadruplet(T1 t1, T2 t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }
        public Quadruplet(T1 t1, T2 t2, T3 t3)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
        }
        public Quadruplet(T2 t2, T3 t3)
        {
            this.t3 = t3;
            this.t2 = t2;
        }
        public Quadruplet(T1 t1, T2 t2, T3 t3, T4 t4)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
            this.t4 = t4;
        }
    }
}
