using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Хранит в себе тип и данные запроса
    /// </summary>
    class ActionData
    {
        public const byte ACTION_CODE_ERROR = 255;
        public const byte ACTION_NOT_FOUND = 0;
        public const byte ACTION_CODE_MESSAGE = 1;
        public const byte ACTION_CODE_GET_CLIENT_NAME = 2;
        public const byte ACTION_CODE_GET_CLIENT_PHOTO = 9;
        public const byte ACTION_CODE_USER_I_AM_TYPING = 3;
        public const byte ACTION_CODE_GET_FILE = 4; 
        public const byte ACTION_CODE_SAY_INPUT_TEXT = 5;
        public const byte ACTION_CODE_UPDATE_INCKCANVAS = 6;
        public const byte ACTION_CODE_ENCRYPTION_MODE_CHANGED = 7;
        public const byte ACTION_CODE_SECRET_WORD_CHANGED = 8;

        /// <summary>
        /// Тип запроса
        /// </summary>
        public byte ActionCode { get; set; }

        /// <summary>
        /// Данные запроса
        /// </summary>
        public Stream Data { get; set; }

        public ActionData()
        {

        }

        public ActionData(byte actionCode, Stream data)
        {
            this.ActionCode = actionCode;
            this.Data = data;
        }

        public ActionData(byte actinCode)
        {
            this.ActionCode = actinCode;
            this.Data = null;
        }


        /// <summary>
        /// Конвертирует массив байт в экземпляр класса
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static ActionData ConvertFromByte(byte[] bytes)
        {
            if (bytes == null) return null;
            byte[] data = new byte[bytes.Length];
            if (bytes.Length > 1)
            {
                Array.Copy(bytes, 1, data, 0, bytes.Length - 1);
            }
            else data = null;
            return new ActionData(bytes[0], new MemoryStream(data));
        }

        /// <summary>
        /// Конвертирует экземпляр класса в массив байт
        /// </summary>
        /// <returns></returns>
        public byte[] ConvertToByte()
        {
            if (this.Data != null)
            {
                byte[] buff = new byte[this.Data.Length + 1];
                this.Data.Read(buff, 1, buff.Length - 1);

                return (new byte[] { this.ActionCode }).Concat<byte>(buff).ToArray();
            }
            else
            {
                return (new byte[] { this.ActionCode });
            }
        }

        /// <summary>
        /// Конвертирует экземпляр класса в поток
        /// </summary>
        /// <returns></returns>
        public MemoryStream ConvertToStream()
        {
            return new MemoryStream(this.ConvertToByte());
        }
    }
}
