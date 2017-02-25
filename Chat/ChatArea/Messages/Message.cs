using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Runtime.Serialization.Formatters.Binary;


namespace Chat
{
    /// <summary>
    /// Объект сообщения
    /// </summary>
    [Serializable]
    public class Message: ISerializable
    {
        #region Public values

        /// <summary>
        /// Прикреплённый файл
        /// </summary>
        public Attachment Attachment { get; set; }

        /// <summary>
        /// Имя автора
        /// </summary>
        public string AutorName { get; set; }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Исходящее сообщение
        /// </summary>
        public bool Out { get; set; }

        /// <summary>
        /// Отправленное сообщение
        /// </summary>
        public bool WasSent { get; set; }

        /// <summary>
        /// Дата отправки
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Привязанный ChatConnection
        /// </summary>
        public ChatConnection chatConnetcion;

        #endregion

        #region Constructors

        public Message()
        {
            this.Out = false;
            this.AutorName = "";
            this.Body = "";
            this.Time = DateTime.Now;
        }
        public Message(string autor, string body, bool Out, Attachment attachment)
        {
            this.AutorName = autor;
            this.Out = Out;
            this.Body = body;
            this.Attachment = attachment;
            this.Time = DateTime.Now;
        }
        public Message(string body, Attachment attachment = null)
        {
            this.AutorName = API.MainServer.UserName;
            this.Body = body;
            this.Out = true;
            this.Attachment = attachment;
            this.Time = DateTime.Now;
        }

        #endregion

        #region Converting

        /// <summary>
        /// Структуризация
        /// </summary>
        /// <param name="data">Массив байт</param>
        /// <param name="startIndex">Индекс элемента, с которого начинать структуризацию</param>
        public void ConventFromBytes(byte[] data, int startIndex = 0)
        {
            if (data == null) throw new ArgumentNullException("data");

            this.AutorName = Utility.Text.Encoding.GetString(data, startIndex);
            startIndex += Utility.Text.Encoding.GetByteCount(data, startIndex);
            this.Body = Utility.Text.Encoding.GetString(data, startIndex);
            startIndex += Utility.Text.Encoding.GetByteCount(data, startIndex);
            if (data[startIndex] == 1)
            {
                this.Attachment = Attachment.ConvertFromByte(data, startIndex + 1);
            }
            else
            {
                this.Attachment = null;
            }
        }

        /// <summary>
        /// Сериализация
        /// </summary>
        /// <returns>Массив байт</returns>
        public byte[] ConventToBytes()
        {
            byte[] result = Utility.Concat(Utility.Text.Encoding.GetBytes(this.AutorName), Utility.Text.Encoding.GetBytes(this.Body), new byte[] { this.Attachment == null ? (byte)0 : (byte)1 });
            if (this.Attachment != null)
            {
                result = Utility.Concat(result, this.Attachment.ConvertToByte());
            }
            return result;
        }

        #endregion

        #region Send message

        public bool Send()
        {
            if (this.WasSent) throw new Exception("Message already sent");

            return API.MainServer.SendMessage(this.chatConnetcion, this);
        }

        #endregion
    }
}
