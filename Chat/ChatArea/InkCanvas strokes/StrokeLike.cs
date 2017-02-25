using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой штрих на доп. доске
    /// </summary>
    [Serializable]
    class StrokeLike: ISerializable
    {
        /// <summary>
        /// Список точек, в которых штрих "изламывается"
        /// </summary>
        public PointList _points = new PointList();

        /// <summary>
        /// Цвет: Alpha, Red, Green, Blue
        /// </summary>
        public Quadruplet<byte, byte, byte, byte> colorStr { get; set; }

        /// <summary>
        /// Сглаживание
        /// </summary>
        public bool firToCurve { get; set; }

        /// <summary>
        /// Высота
        /// </summary>
        public double height { get; set; }

        /// <summary>
        /// Ширина
        /// </summary>
        public double width { get; set; }

        /// <summary>
        /// Сериализация
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] ConventToBytes()
        {
            byte[] result = Utility.Concat(Utility.Convent.GetBytes(width), Utility.Convent.GetBytes(height));
            result = Utility.Concat(result, Utility.Convent.GetBytes(firToCurve));
            result = Utility.Concat(result, new byte[] { colorStr.t1 }, new byte[] { colorStr.t2 }, new byte[] { colorStr.t3 }, new byte[] { colorStr.t4 });
            result = Utility.Concat(result, _points.ConventToBytes());
            return result;
        }

        /// <summary>
        /// Структуизация
        /// </summary>
        /// <param name="bytes">Массив байт</param>
        /// <param name="startIndex">Индекс элемента, с которго начиать структуризацию</param>
        public void ConventFromBytes(byte[] bytes, int startIndex = 0)
        {
            this.width = Utility.Convent.ToDouble(bytes, startIndex);
            startIndex += sizeof(double);
            this.height = Utility.Convent.ToDouble(bytes, startIndex);
            startIndex += sizeof(double);
            this.firToCurve = Utility.Convent.ToBool(bytes, startIndex);
            startIndex += sizeof(byte);

            this.colorStr = new Quadruplet<byte, byte, byte, byte>();
            this.colorStr.t1 = Utility.Convent.ToByte(bytes, startIndex);
            startIndex += sizeof(byte);
            this.colorStr.t2 = Utility.Convent.ToByte(bytes, startIndex);
            startIndex += sizeof(byte);
            this.colorStr.t3 = Utility.Convent.ToByte(bytes, startIndex);
            startIndex += sizeof(byte);
            this.colorStr.t4 = Utility.Convent.ToByte(bytes, startIndex);
            startIndex += sizeof(byte);

            this._points.ConventFromBytes(bytes, startIndex);
            return;
        }
    }
}
