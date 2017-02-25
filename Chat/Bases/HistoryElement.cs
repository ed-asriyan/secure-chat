using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Представляет собой элемент в истории запросов
    /// </summary>
    class HistoryElement
    {
        /// <summary>
        /// IP-адрес
        /// </summary>
        public IPAddress IpAddress { get; private set; }

        /// <summary>
        /// Дата запроса
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Тип запроса
        /// </summary>
        public byte RequestType { get; set; }

        /// <summary>
        /// Подтип запроса
        /// </summary>
        public byte RequestSubtype { get; set; }

        /// <summary>
        /// Положительный ли ответ
        /// </summary>
        public bool GoodResponse { get; set; }

        #region Constructors

        public HistoryElement()
        {

        }

        public HistoryElement(IPAddress ipAddress, byte requestType, byte requestSubtype, bool goodResponse)
        {
            this.IpAddress = ipAddress;
            this.RequestSubtype = requestSubtype;
            this.RequestType = requestType;
            this.GoodResponse = goodResponse;
            this.Time = DateTime.Now;
        }

        #endregion

        #region ToString

        public string ToString()
        {
            var result = string.Empty;
            try
            {

                result += "[" + this.Time.ToShortTimeString() + "] ";
                result += this.IpAddress.ToString().PadRight(15) + ": ";
                switch (this.RequestType)
                {
                    #region Message

                    case ActionData.ACTION_CODE_MESSAGE:
                        result += "Отправил сообщение.";
                        break;
                    #endregion

                    #region Client Name

                    case ActionData.ACTION_CODE_GET_CLIENT_NAME:
                        result += "Запрос на имя.";
                        break;

                    #endregion

                    #region Client photo

                    case ActionData.ACTION_CODE_GET_CLIENT_PHOTO:
                        result += "Запрос на фото.";
                        break;

                    #endregion

                    #region Typing

                    case ActionData.ACTION_CODE_USER_I_AM_TYPING:
                        result += "Начал набирать сообщение.";
                        break;

                    #endregion

                    #region Attachment

                    case ActionData.ACTION_CODE_GET_FILE:
                        result += "Запрос на скачивание файла.";
                        break;

                    #endregion

                    #region Input text

                    case ActionData.ACTION_CODE_SAY_INPUT_TEXT:
                        result += "Отправил текст на доп. доске.";
                        break;

                    #endregion

                    #region InckCanvas

                    case ActionData.ACTION_CODE_UPDATE_INCKCANVAS:
                        switch (this.RequestSubtype)
                        {
                            case API.TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_BACKGROUND:
                                result += "Отправил избражение фона холста.";
                                break;
                            case API.TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_SIZE:
                                result += "Отправил размер холста.";
                                break;
                            case API.TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_STROKES:
                                result += "Отправил список штрихов на холсте.";
                                break;
                            default:
                                result += "Отправил неизвестный подтип обновения холста, код " + this.RequestSubtype.ToString() + ".";
                                break;
                        }
                        break;

                    #endregion

                    default:
                        result += "Неизвестный запрос, код " + this.RequestType.ToString() + ".";
                        break;
                }

                result += " Ответ " + (this.GoodResponse ? "положительный" : "отрицательный");

            }
            catch
            {
                result = base.ToString();
            }
            return result;
        }

        #endregion
    }
}
