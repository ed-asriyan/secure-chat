using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
namespace Chat
{
    /// <summary>
    /// Статический класс содержит вспомогательные методы
    /// </summary>
    static class Utility
    {
        static Random random = new Random();

        #region Types conventer

        /// <summary>
        /// Конвертирует данные из одного типа в другой
        /// </summary>
        static public class Convent
        {
            /// <summary>
            /// Конвертирует массив байт в Int32
            /// </summary>
            /// <param name="data">Массив, который надо конвертировать</param>
            /// <param name="startIndex">Индекс элемента, с которого надо начать конвертирование</param>
            /// <returns>Результат конвертирования</returns>
            static public Int32 ToInt32(byte[] data, int startIndex = 0){
                byte[] valueBytes = new byte[sizeof(Int32)];
                Array.Copy(data, startIndex, valueBytes, 0, sizeof(Int32));
                
                if (BitConverter.IsLittleEndian) valueBytes = valueBytes.Reverse().ToArray();
                return BitConverter.ToInt32(valueBytes, 0);
            }

            /// <summary>
            /// Конвертирует массив байт в Int64
            /// </summary>
            /// <param name="data">Массив, который надо конвертировать</param>
            /// <param name="startIndex">Индекс элемента, с которого надо начать конвертирование</param>
            /// <returns>Результат конвертирования</returns>
            static public Int64 ToInt64(byte[] data, int startIndex = 0)
            {
                byte[] valueBytes = new byte[sizeof(Int64)];
                Array.Copy(data, startIndex, valueBytes, 0, sizeof(Int64));

                if (BitConverter.IsLittleEndian) valueBytes = valueBytes.Reverse().ToArray();
                return BitConverter.ToInt64(valueBytes, 0);
            }

            /// <summary>
            /// Конвертирует массив байт в double
            /// </summary>
            /// <param name="data">Массив, который надо конвертировать</param>
            /// <param name="startIndex">Индекс элемента, с которого надо начать конвертирование</param>
            /// <returns>Результат конвертирования</returns>
            static public double ToDouble(byte[] data, int startIndex = 0)
            {
                byte[] valueBytes = new byte[sizeof(double)];
                Array.Copy(data, startIndex, valueBytes, 0, sizeof(double));

                return BitConverter.ToDouble(valueBytes, 0);
            }

            /// <summary>
            /// Конвертирует Int32 в массив байт
            /// </summary>
            /// <param name="value">Значение, которое надо конвертировать</param>
            /// <returns>Результат конвертирования</returns>
            static public byte[] GetBytes(Int32 value){
                var result = BitConverter.GetBytes(value);
                return BitConverter.IsLittleEndian ? result.Reverse().ToArray() : result;
            }

            /// <summary>
            /// Конвертирует Int64 в массив байт
            /// </summary>
            /// <param name="value">Значение, которое надо конвертировать</param>
            /// <returns>Результат конвертирования</returns>
            static public byte[] GetBytes(Int64 value)
            {
                var result = BitConverter.GetBytes(value);
                return BitConverter.IsLittleEndian ? result.Reverse().ToArray() : result;
            }

            /// <summary>
            /// Конвертирует bool в массив байт
            /// </summary>
            /// <param name="value">Значение, которое надо конвертировать</param>
            /// <returns>Результат конвертирования</returns>
            static public byte[] GetBytes(bool value)
            {
                return BitConverter.GetBytes(value);
            }

            /// <summary>
            /// Конвертирует double в массив байт
            /// </summary>
            /// <param name="value">Значение, которое надо конвертировать</param>
            /// <returns>Результат конвертирования</returns>
            static public byte[] GetBytes(double value)
            {
                return BitConverter.GetBytes(value);
            }

            /// <summary>
            /// Конвертирует массив байт в bool
            /// </summary>
            /// <param name="data">Массив, который надо конвертировать</param>
            /// <param name="startIndex">Индекс элемента, с которого надо начать конвертирование</param>
            /// <returns>Результат конвертирования</returns>
            static public bool ToBool(byte[] value, int startIndex = 0)
            {
                return BitConverter.ToBoolean(value, startIndex);
            }

            /// <summary>
            /// Конвертирует массив байт в byte
            /// </summary>
            /// <param name="data">Массив, который надо конвертировать</param>
            /// <param name="startIndex">Индекс элемента, с которого надо начать конвертирование</param>
            /// <returns>Результат конвертирования</returns>
            static public byte ToByte(byte[] value, int startIndex = 0)
            {
                return value[startIndex];
            }
        }

        #endregion

        #region Text

        /// <summary>
        /// Помогает в работе со строками
        /// </summary>
        static public class Text
        {
            /// <summary>
            /// Кодирует и декодирует строки
            /// </summary>
            static public class Encoding
            {
                /// <summary>
                /// Конвертирует массив байт в строку
                /// </summary>
                /// <param name="bytes">Массив, который надо конвертировать</param>
                /// <param name="startIndex">Индекс элемента в массиве, с которого надо начать конвертирование</param>
                /// <returns>Результат конвертирования</returns>
                static public string GetString(byte[] bytes, int startIndex = 0)
                {
                    int count = Utility.Convent.ToInt32(bytes, startIndex);
                    return System.Text.Encoding.UTF8.GetString(bytes, sizeof(int) + startIndex, count);
                }

                /// <summary>
                /// Конвертирует строку в массив байт
                /// </summary>
                /// <param name="str">Строка, которую надо конвертировать</param>
                /// <returns>Результат конвертирования</returns>
                static public byte[] GetBytes(string str)
                {
                    byte[] coutn = Utility.Convent.GetBytes(System.Text.Encoding.UTF8.GetByteCount(str)); //Utility.Convent.GetBytes(str.Length * sizeof(char));
                    return Utility.Concat(coutn, System.Text.Encoding.UTF8.GetBytes(str));
                }

                /// <summary>
                /// Вычисляет размер массива, который являлся бы результатом конвертирования строки
                /// </summary>
                /// <param name="str">Строка, на основе которой надо провести вычисление</param>
                /// <returns>Размер массива, который являлся бы результатом конвертирования строки</returns>
                static public int GetByteCount(string str)
                {
                    return System.Text.Encoding.UTF8.GetByteCount(str) + sizeof(int);
                }
                
                /// <summary>
                /// Вычисляет кол-во байт, которое занимает строка в массиве
                /// </summary>
                /// <param name="bytes">Массив в котром находится строка</param>
                /// <param name="startIndex">Индект элемента, с которого начинается строка</param>
                /// <returns></returns>
                static public int GetByteCount(byte[] bytes, int startIndex = 0)
                {
                    return Utility.Convent.ToInt32(bytes, startIndex) + sizeof(int);
                }
            }

            /// <summary>
            /// Представляет размер файла в читабельную строку
            /// </summary>
            /// <param name="size">Размер файла в байтах</param>
            /// <param name="padding">Длина числительного в результативной строке</param>
            /// <returns>Строка, показывающая размер файла в читабельном виде</returns>
            static public string FileSizeToString(long size, int? padding = null)
            {
                if (padding != null && padding.Value <= 0) throw new ArgumentOutOfRangeException("padding");

                long tmp = size;
                int r = 0;
                while (tmp > 0)
                {
                    r++;
                    tmp /= 1000;
                }
                string st;
                switch (r)
                {
                    case 0:
                    case 1:
                        st = "";
                        break;
                    case 2:
                        st = "К";
                        break;
                    case 3:
                        st = "М";
                        break;
                    case 4:
                        st = "Г";
                        break;
                    case 5:
                        st = "Т";
                        break;
                    default:
                        st = "П";
                        break;
                }
                string text = (size / System.Math.Pow(10, (3 * (r - 1)))).ToString();
                int pad = padding != null ? padding.Value : text.Length;
                text = text.PadRight(pad).Substring(0, pad);
                if (text[text.Length - 1] == ',') text = text.Remove(text.Length - 1, 1) + ' ';

                return text + " " + st + "Б";
            }
        }

        #endregion

        #region UI

        /// <summary>
        /// Помогает в работе с элементами управления
        /// </summary>
        public static class UI
        {
            /// <summary>
            /// Делает скриншот контрола
            /// </summary>
            /// <param name="visual">Контрол, скриншот которого надо сделать</param>
            /// <returns>Поток содержащий скриншот</returns>
            static public MemoryStream GetScreenshot(Visual visual)
            {
                MemoryStream ms = null;
                visual.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        Rect bounds = VisualTreeHelper.GetDescendantBounds(visual);
                        RenderTargetBitmap bitmap = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
                        bitmap.Render(visual);
                        PngBitmapEncoder image = new PngBitmapEncoder();
                        image.Frames.Add(BitmapFrame.Create(bitmap));
                        ms = new MemoryStream();
                        image.Save(ms);
                    }
                    catch { }
                });
                return ms;
            }

            /// <summary>
            /// Начинает анимацию угасания или появления элемента
            /// </summary>
            /// <param name="frameworkElement">Контрол, над котроым начинается анимация</param>
            /// <param name="visible">TRUE - анимация появления, FALSE - анимация угасания</param>
            /// <param name="duration">Продолжительность анимации (мс)</param>
            /// <param name="beginTime">Кол-во милисек., спустя которое начать анимацию</param>
            /// <param name="to">Конечное значение Opacity элемента frameworkElement</param>
            /// <param name="onComplete">Метод, вызываемый после окончания анимации</param>
            public static void BeginVisibilityAnimation(FrameworkElement frameworkElement, bool visible, uint duration, uint beginTime = 0, double? to = null, Action onComplete = null)
            {
                if (frameworkElement == null) throw new ArgumentNullException("frameworkElement");

                frameworkElement.Dispatcher.Invoke(() =>
                {
                    double from = visible ? 0 : 1;
                    if (to == null) to = visible ? 1 : 0;

                    System.Windows.Media.Animation.DoubleAnimation anim = new System.Windows.Media.Animation.DoubleAnimation(from, to.Value, TimeSpan.FromMilliseconds(duration));

                    anim.BeginTime = TimeSpan.FromMilliseconds(beginTime);

                    anim.Completed += (object sender, EventArgs e) =>
                    {
                        try
                        {
                            if (to == 0)
                            {
                                frameworkElement.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                            }
                        }
                        catch { }

                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    };

                    if (visible)
                    {
                        frameworkElement.Opacity = 0;
                        frameworkElement.Visibility = Visibility.Visible;
                    }

                    frameworkElement.BeginAnimation(FrameworkElement.OpacityProperty, anim);
                });
            }

            /// <summary>
            /// Начинает анимацию измениения высоты контрола
            /// </summary>
            /// <param name="frameworkElement">Контрол, над котроым начинается анимация</param>
            /// <param name="visible">TRUE - анимация появления, FALSE - анимация исчезания</param>
            /// <param name="duration">Продолжительность анимации (мс)</param>
            /// <param name="beginTime">Кол-во милисек., спустя которое начать анимацию</param>
            /// <param name="onComplete">Метод, вызываемый после окончания анимации</param>
            public static void BeginHeightAnimation(FrameworkElement frameworkElement, bool visible, uint duration, uint beginTime = 0, Action onComplete = null)
            {
                if (frameworkElement == null) throw new ArgumentNullException("frameworkElement");;

                frameworkElement.Dispatcher.Invoke(() =>
                {
                    double from = visible ? 0 : 1;
                    double to = visible ? 1 : 0;

                    System.Windows.Media.Animation.DoubleAnimation anim = new System.Windows.Media.Animation.DoubleAnimation(to, TimeSpan.FromMilliseconds(duration));

                    anim.BeginTime = TimeSpan.FromMilliseconds(beginTime);

                    System.Windows.Media.ScaleTransform sc = new ScaleTransform();

                  /*  if (frameworkElement.LayoutTransform != null && frameworkElement.LayoutTransform is ScaleTransform)
                    {
                        var sc0 = (frameworkElement.LayoutTransform as ScaleTransform);
                        sc.ScaleY = sc0.ScaleY;
                        sc.ScaleX = sc0.ScaleX;
                        sc.CenterX = sc0.ScaleX;
                        sc.ScaleY = sc0.CenterY;
                    }
                    else
                    {
                        sc.ScaleY = from;
                        sc.ScaleX = 1;
                    } */sc.ScaleY = from;

                    frameworkElement.LayoutTransform = sc;

                    anim.Completed += (object sender, EventArgs e) =>
                    {
                        frameworkElement.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    };

                    if (visible)
                    {
                        frameworkElement.Visibility = Visibility.Visible;
                    }

                    sc.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                });
            }

            /// <summary>
            /// Начинает анимацию измениения ширины контрола
            /// </summary>
            /// <param name="frameworkElement">Контрол, над котроым начинается анимация</param>
            /// <param name="visible">TRUE - анимация появления, FALSE - анимация исчезания</param>
            /// <param name="duration">Продолжительность анимации (мс)</param>
            /// <param name="beginTime">Кол-во милисек., спустя которое начать анимацию</param>
            /// <param name="onComplete">Метод, вызываемый после окончания анимации</param>
            public static void BeginWidthAnimation(FrameworkElement frameworkElement, bool visible, uint duration, uint beginTime = 0, Action onComplete = null)
            {
                if (frameworkElement == null) throw new ArgumentNullException("frameworkElement"); ;

                frameworkElement.Dispatcher.Invoke(() =>
                {
                    double from = visible ? 0 : 1;
                    double to = visible ? 1 : 0;

                    System.Windows.Media.Animation.DoubleAnimation anim = new System.Windows.Media.Animation.DoubleAnimation(to, TimeSpan.FromMilliseconds(duration));

                    anim.BeginTime = TimeSpan.FromMilliseconds(beginTime);

                    System.Windows.Media.ScaleTransform sc = new ScaleTransform();

                    /*  if (frameworkElement.LayoutTransform != null && frameworkElement.LayoutTransform is ScaleTransform)
                      {
                          var sc0 = (frameworkElement.LayoutTransform as ScaleTransform);
                          sc.ScaleY = sc0.ScaleY;
                          sc.ScaleX = sc0.ScaleX;
                          sc.CenterX = sc0.ScaleX;
                          sc.ScaleY = sc0.CenterY;
                      }
                      else
                      {
                          sc.ScaleY = from;
                          sc.ScaleX = 1;
                      } */
                    sc.ScaleX = 0;

                    frameworkElement.LayoutTransform = sc;

                    anim.Completed += (object sender, EventArgs e) =>
                    {
                        frameworkElement.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    };

                    if (visible)
                    {
                        frameworkElement.Visibility = Visibility.Visible;
                    }

                    sc.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                });
            }

            /// <summary>
            /// Начинает анимацию измениения ширины и высоты контрола
            /// </summary>
            /// <param name="frameworkElement">Контрол, над котроым начинается анимация</param>
            /// <param name="visible">TRUE - анимация появления, FALSE - анимация исчезания</param>
            /// <param name="duration">Продолжительность анимации (мс)</param>
            /// <param name="beginTime">Кол-во милисек., спустя которое начать анимацию</param>
            /// <param name="onComplete">Метод, вызываемый после окончания анимации</param>
            public static void BeginHeightAndWidthAnimation(FrameworkElement frameworkElement, bool visible, uint duration, uint beginTime = 0, Action onComplete = null)
            {
                if (frameworkElement == null) throw new ArgumentNullException("frameworkElement"); ;

                frameworkElement.Dispatcher.Invoke(() =>
                {
                    double from = visible ? 0 : 1;
                    double to = visible ? 1 : 0;

                    System.Windows.Media.Animation.DoubleAnimation anim = new System.Windows.Media.Animation.DoubleAnimation(to, TimeSpan.FromMilliseconds(duration));

                    anim.BeginTime = TimeSpan.FromMilliseconds(beginTime);

                    System.Windows.Media.ScaleTransform sc = new ScaleTransform();

                    /*  if (frameworkElement.LayoutTransform != null && frameworkElement.LayoutTransform is ScaleTransform)
                      {
                          var sc0 = (frameworkElement.LayoutTransform as ScaleTransform);
                          sc.ScaleY = sc0.ScaleY;
                          sc.ScaleX = sc0.ScaleX;
                          sc.CenterX = sc0.ScaleX;
                          sc.ScaleY = sc0.CenterY;
                      }
                      else
                      {
                          sc.ScaleY = from;
                          sc.ScaleX = 1;
                      } */
                    sc.ScaleX = 0;
                    sc.ScaleY = 0;

                    frameworkElement.LayoutTransform = sc;

                    anim.Completed += (object sender, EventArgs e) =>
                    {
                        frameworkElement.Visibility = visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                        if (onComplete != null)
                        {
                            onComplete();
                        }
                    };

                    if (visible)
                    {
                        frameworkElement.Visibility = Visibility.Visible;
                    }

                    sc.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    sc.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                });
            }
        }

        #endregion

        /// <summary>
        /// Соединяет несколько массивов в один
        /// </summary>
        /// <typeparam name="T">Тип элементов массива</typeparam>
        /// <param name="elements">Массивы, которые надо объединить</param>
        /// <returns>Массив, полученный в результате объединения массивов</returns>
        static public T[] Concat<T>(params T[][] elements){
            T[] result = new T[] {};
            foreach (var element in elements)
            {
                result = result.Concat<T>(element).ToArray();
            }
            if (result == null) throw new Exception("Some error");
            return result;
        }

        /// <summary>
        /// Проверяет идентичность массивов
        /// </summary>
        /// <param name="bytes1">Первый массив</param>
        /// <param name="bytes2">Второй массив</param>
        /// <returns></returns>
        static public bool ByteArrayCompare(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length)
                return false;

            var length = bytes1.Length;

            for (int i = 0; i < length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Генерирует случайное целое число
        /// </summary>
        /// <param name="min">Минимальное возможное число</param>
        /// <param name="max">Максимальное возможное число</param>
        /// <returns>Случайное число в заданном диапазоне</returns>
        static public int RandomInt(int? min = null, int? max = null)
        {
            if (min == null) min = int.MinValue;
            if (max == null) max = int.MaxValue;
            return random.Next(min.Value, max.Value);
        }

        /// <summary>
        /// Генеринует случайную последовательность байт
        /// </summary>
        /// <param name="length">Днина последовательности</param>
        /// <returns>Массив, сосотоящий из случайной последовательности байт</returns>
        static public byte[] RandomBytes(uint length)
        {
            if (length <= 0) throw new ArgumentOutOfRangeException("length", "Must be positive number");
            var randBytes = new byte[length];
            random.NextBytes(randBytes);
            return randBytes;
        }
    }
}
