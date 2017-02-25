using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;

namespace Chat
{
    /// <summary>
    /// Представляет собой список штрихов, способный сериализоваться
    /// </summary>
    [Serializable]
    class StrokeLikeList: ISerializable
    {
        public List<StrokeLike> list = new List<StrokeLike>(); // список штрихов

        #region Constructors

        public StrokeLikeList()
        {

        }
        public StrokeLikeList(IEnumerable<StrokeLike> list)
        {
            this.list = list as List<StrokeLike>;
        }

        public StrokeLikeList(StrokeCollection strokeCollection)
        {
            foreach (var stroke in strokeCollection)
            {
                PointList points = new PointList();

                foreach (var point in stroke.StylusPoints)
                {
                    points.list.Add(new Point(point.X, point.Y));
                }

                StrokeLike sl = new StrokeLike();
                sl._points = points;
                sl.width = stroke.DrawingAttributes.Width;
                sl.height = stroke.DrawingAttributes.Height;
                sl.firToCurve = stroke.DrawingAttributes.FitToCurve;

                sl.colorStr = new Quadruplet<byte, byte, byte, byte>(stroke.DrawingAttributes.Color.A, stroke.DrawingAttributes.Color.R, stroke.DrawingAttributes.Color.G, stroke.DrawingAttributes.Color.B);

                this.list.Add(sl);
            }
        }

        #endregion

        /// <summary>
        /// Ковертирует в объект StrokeCollection
        /// </summary>
        /// <returns>Объект StrokeCollection</returns>
        public StrokeCollection ConventToStrokeCollection()
        {
            StrokeCollection strokeCollection = new StrokeCollection();

            foreach (var stroke in this.list)
            {
                StylusPointCollection stCollection = new StylusPointCollection();

                foreach (var point in stroke._points.list)
                {
                    stCollection.Add(new StylusPoint(point.X, point.Y));
                }

                Stroke _stroke = new Stroke(stCollection);

                var color = new System.Windows.Media.Color();
               color.A = stroke.colorStr.t1;
                color.R = stroke.colorStr.t2;
                color.G = stroke.colorStr.t3;
                color.B = stroke.colorStr.t4;
                _stroke.DrawingAttributes.Color = color;
                _stroke.DrawingAttributes.Height = stroke.height;
                _stroke.DrawingAttributes.Width = stroke.width;

                _stroke.DrawingAttributes.FitToCurve = stroke.firToCurve;

                strokeCollection.Add(_stroke);
            }
            return strokeCollection;
        }

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
        /// Структуризиция
        /// </summary>
        /// <param name="bytes">Массив байт</param>
        /// <param name="startIndex">Индекс элемента, с котрого начинать структуризацию</param>
        public void ConventFromBytes(byte[] bytes, int startIndex)
        {
            List<StrokeLike> list = new List<StrokeLike>();

            int count = Utility.Convent.ToInt32(bytes, startIndex);
            startIndex += sizeof(int);

            for (int i = 0; i < count; i++)
            {
                int countB = Utility.Convent.ToInt32(bytes, startIndex);
                startIndex += sizeof(int);
                byte[] t = new byte[countB];
                Array.Copy(bytes, startIndex, t, 0, t.Length);
                StrokeLike p = new StrokeLike();
                p.ConventFromBytes(t, 0);
                list.Add(p);
                startIndex += countB;
            }

            this.list = list;
        }
    }
}
