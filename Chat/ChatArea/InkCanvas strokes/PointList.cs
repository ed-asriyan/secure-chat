using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    /// <summary>
    /// Представляет собой список точек
    /// </summary>
    [Serializable]
    class PointList : ISerializable
    {
        public List<Point> list = new List<Point>();

        #region Constructors

        public PointList()
        {

        }
        public PointList(IEnumerable<Point> list)
        {
            this.list = list as List<Point>;
        }

        #endregion


        /// <summary>
        /// Сериализация
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] ConventToBytes()
        {
            List<byte> result = new List<byte>();
            result.AddRange(Utility.Convent.GetBytes(list.Count));
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    result.AddRange(Utility.Convent.GetBytes((int)0));
                    continue;
                }
                byte[] bytes = list[i].ConventToBytes();
                result.AddRange(Utility.Convent.GetBytes(bytes.Length));

                result.AddRange(bytes);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Структуризация
        /// </summary>
        /// <param name="bytes">Массив байт</param>
        /// <param name="startIndex">Индекс элемента с которого начать структуризацию</param>
        public void ConventFromBytes(byte[] bytes, int startIndex = 0)
        {
            List<Point> list = new List<Point>();

            int count = Utility.Convent.ToInt32(bytes, startIndex);
            startIndex += sizeof(int);

            for (int i = 0; i < count; i++)
            {
                int countB = Utility.Convent.ToInt32(bytes, startIndex);
                startIndex += sizeof(int);
                byte[] t = new byte[countB];
                Array.Copy(bytes, startIndex, t, 0, t.Length);
                Point p = new Point();
                p.ConventFromBytes(t, 0);
                list.Add(p);
                startIndex += countB;
            }

            this.list = list;
        }
    }
}