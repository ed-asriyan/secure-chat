using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой точку излома штриха
    /// </summary>
    [Serializable]
    class Point : ISerializable
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Point()
        {

        }
        public Point(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Структуризация
        /// </summary>
        /// <param name="bytes">Массив байт</param>
        /// <param name="startIndex">Индекс элемента, с которого начать структуризацию</param>
        public void ConventFromBytes(byte[] bytes, int startIndex)
        {
            byte[] xx = new byte[sizeof(double)];
            byte[] yy =  new byte[sizeof(double)];
 
            Array.Copy(bytes, startIndex, xx, 0, sizeof(double));
            Array.Copy (bytes, startIndex + sizeof(double), yy, 0, sizeof(double));

            this.X = Utility.Convent.ToDouble(xx);
            this.Y = Utility.Convent.ToDouble(yy);
        }

        /// <summary>
        /// Сериализация
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] ConventToBytes()
        {
            return Utility.Concat(Utility.Convent.GetBytes(this.X), Utility.Convent.GetBytes(this.Y));
        }
    }
}