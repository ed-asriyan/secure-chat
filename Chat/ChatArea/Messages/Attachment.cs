using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization.Formatters.Binary;

namespace Chat
{
    /// <summary>
    /// Объект прикреплённого файла
    /// </summary>
    [Serializable]
    public class Attachment
    {
        #region Only for use on local client

        public string Path = null;

        #endregion

        #region Public values

        /// <summary>
        /// Расширение
        /// </summary>
        public string extession
        {
            get
            {
                return System.IO.Path.GetExtension(this.Name);
            }
        }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// IP
        /// </summary>
        public string IP { get; set; }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Размер
        /// </summary>
        public long size { get; set; }

        /// <summary>
        /// Кол-во скачиваний
        /// </summary>
        public uint DownloadCount { get; set; }

        #endregion

        #region Constructors

        public Attachment() { }

        #endregion

        #region Converting

        /// <summary>
        /// Сериализация
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] ConvertToByte()
        {
            byte[] result = Utility.Concat(Utility.Text.Encoding.GetBytes(this.Id), Utility.Text.Encoding.GetBytes(this.Name), Utility.Convent.GetBytes(this.size));
            return result;
        }

        /// <summary>
        /// Структуризация
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <param name="startIndex">Индекс элемента, с которого начинать структуризацию</param>
        /// <returns></returns>
        public static Attachment ConvertFromByte(byte[] data, int startIndex = 0)
        {
            if (data == null) throw new ArgumentNullException("data");

            Attachment result = new Attachment();
            result.Id = Utility.Text.Encoding.GetString(data, startIndex);
            startIndex += Utility.Text.Encoding.GetByteCount(data, startIndex);
            result.Name = Utility.Text.Encoding.GetString(data, startIndex);
            startIndex += Utility.Text.Encoding.GetByteCount(data, startIndex);
            result.size = Utility.Convent.ToInt64(data, startIndex);
            
            return result;
        }

        #endregion
    }
}