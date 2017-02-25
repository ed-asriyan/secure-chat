using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Threading;
using System.Windows.Ink;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Windows.Input;

using System.Net;

namespace Chat
{
    /// <summary>
    /// Обеспечивает логику чата
    /// </summary>
    class API
    {
        #region Types Handlers

        /// <summary>
        /// Обработчики запросов
        /// </summary>
        internal static class TypesHandlers
        {
            #region Message

            /// <summary>
            /// Обработчик сообщений
            /// </summary>
            internal static class MessageHandler
            {
                public const byte CODE_RECEVE_OK = 0;
                public const byte CODE_RECIEVE_ERROR = 1;

                /// <summary>
                /// Обработчик отправки сообщения
                /// </summary>
                /// <param name="message">Отправляемое сообщение</param>
                /// <returns>Поток, содержащий запрос на новое сообщение</returns>
                static public Stream SendHandler(Message message)
                {
                    return new MemoryStream(message.ConventToBytes());
                }

                /// <summary>
                /// Обработчик принятия и обработки полученного сообщения
                /// </summary>
                /// <param name="bytes">Полученные данные</param>
                /// <param name="ipAddress">IP-адрес отправителя</param>
                /// <returns>Поток, предназначенный для ответа клиенту</returns>
                static public Stream RecieveHandler(Stream bytes, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_MESSAGE, 0, false));
                    if (!Settings.Global.recieveMessagesFromOthers && !ConnectionBase.Main.Contains(ipAddress))
                    { // если пользователь не хочет получать сообщения от посторонних людей
                        hElement.GoodResponse = true;
                        return new MemoryStream(new byte[] { CODE_RECEVE_OK }); // говорим клиенту, что сообщение якобы успешно принято
                    }

                    Message message = new Message();
                    message.Out = false;
                    message.WasSent = true;
                    try
                    {
                        byte[] bytesB = new byte[bytes.Length];
                        bytes.Position = 1;
                        bytes.Read(bytesB, 0, bytesB.Length);
                        message.ConventFromBytes(bytesB);
                    }
                    catch { }

                    if (message != null)
                    {
                        if (API.MainServer.OnMessageRecieve != null)
                        {
                            // Генерируем событие нового сообщения
                            API.MainServer.OnMessageRecieve.BeginInvoke(API.MainServer, new RecievedMessageEventArgs(message, ipAddress), null, null);
                        }
                        hElement.GoodResponse = true;
                        return new MemoryStream(new byte[] { CODE_RECEVE_OK }); // говорим клиенту, что сообщение принято
                    }
                    else
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream( new byte[] { CODE_RECIEVE_ERROR }); // говорим, что сообщение принято некорректно
                    }
                }

                /// <summary>
                /// Обработчик ответа
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <returns>Результат, полученный от сервера</returns>
                static public bool ResponseHandler(Stream bytes)
                {
                    if (bytes == null || bytes.Length == 0) return false;
                    bytes.Position = 0;
                    return bytes.ReadByte() == CODE_RECEVE_OK;
                }
            }

            #endregion

            #region Client name

            /// <summary>
            /// Обработчик запроса на имя пользователя
            /// </summary>
            static internal class ClientNameHandler
            {
                public const byte CODE_SEND_REQUEST = 0;
                public const byte CODE_RESPONCE_OK = 1;
                public const byte CODE_RESPONCE_ERROR = 2;

                /// <summary>
                /// Обработчик исходящего запроса
                /// </summary>
                /// <returns>Поток, содержащий исходящий запрос</returns>
                static public Stream SendRequestHandler()
                {
                    return new MemoryStream(new byte[] { CODE_SEND_REQUEST });
                }

                /// <summary>
                /// Обработчик принятого запроса
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <param name="ipAddress">IP-адрес клиента</param>
                /// <returns>Поток, содержащий ответ на запрос</returns>
                static public Stream ReceiveHandler(Stream bytes, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_GET_CLIENT_NAME, CODE_SEND_REQUEST, false));
                    bytes.Position = 0;
                    if (bytes == null || bytes.Length == 0 || (hElement.RequestSubtype = (byte)bytes.ReadByte()) != CODE_SEND_REQUEST)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONCE_ERROR });
                    }

                    hElement.GoodResponse = true;
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_RESPONCE_OK }, Utility.Text.Encoding.GetBytes(Settings.Global.UserName)));
                }

                /// <summary>
                /// Обработчик ответа на запрос
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <returns>Имя собеседника</returns>
                static public string ResponseHandler(Stream bytes)
                {
                    bytes.Position = 0;
                    if (bytes == null || bytes.Length == 0 || bytes.ReadByte() != CODE_RESPONCE_OK)
                    {
                        return null;
                    }

                    try
                    {
                        byte[] buff = new byte[bytes.Length];
                        bytes.Position = 0;
                        bytes.Read(buff, 0, buff.Length);
                        return Utility.Text.Encoding.GetString(buff, 1);
                    }
                    catch { return null; }
                }
            }

            #endregion

            #region Client photo

            /// <summary>
            /// Обработчик запроса на фото пользователя
            /// </summary>
            static internal class ClientPhotoHandler
            {
                public const byte CODE_REQUEST = 0;
                public const byte CODE_RESPONSE_OK = 1;
                public const byte CODE_RESPONSE_ERROR = 2;

                /// <summary>
                /// Обработчки исходящего запроса
                /// </summary>
                /// <returns>Поток, содержащий исходящий запрос</returns>
                static public Stream SendRequestHandler()
                {
                    return new MemoryStream(new byte[] { CODE_REQUEST });
                }

                /// <summary>
                /// Обработчик принятого запроса
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <param name="ipAddress">ip-адрес клиента</param>
                /// <returns>Поток, содержащий ответ на запрос</returns>
                static public Stream ReceiveHandler(Stream bytes, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_GET_CLIENT_PHOTO, CODE_REQUEST, false));
                    if (bytes == null || bytes.Length == 0)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    hElement.GoodResponse = true;
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_RESPONSE_OK }, Settings.Global.UserPhotoBytes));
                }

                /// <summary>
                /// Обработчик ответа на запрос
                /// </summary>
                /// <param name="response">Принятые данные</param>
                /// <returns>Массив байт, содержащий фото пользователя</returns>
                static public byte[] ResponseHandler(Stream response)
                {
                    if (response == null || response.Length == 0)
                    {
                        return null;
                    }

                    response.Position = 0;
                    if (response.ReadByte() == CODE_RESPONSE_OK)
                    {
                        byte[] result = new byte[response.Length - 1];
                        response.Read(result, 0, result.Length);
                        return result;
                    }
                    else
                    {
                        return null;
                    }

                }
            }

            #endregion

            #region Typing

            /// <summary>
            /// Обработчик запроса на уведомление о наборе сообщения
            /// </summary>
            static internal class TypingHandler
            {
                public const byte CODE_REQUEST = 0;
                public const byte CODE_RESPONSE_OK = 1;
                public const byte CODE_RESPONSE_ERROR = 2;
                
                /// <summary>
                /// Обработчик исходящего запроса
                /// </summary>
                /// <returns>Поток, содержащий исходящий запрос</returns>
                static public Stream SendRequestHandler()
                {
                    return new MemoryStream(new byte[] { CODE_REQUEST });
                }

                /// <summary>
                /// Обработчик принятого запроса
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <param name="ipAddress">IP-адрес клиента</param>
                /// <returns>Поток, содержащий ответ на запрос</returns>
                static public Stream RecieveHandler(Stream bytes, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_USER_I_AM_TYPING, CODE_REQUEST, false));
                    if (bytes == null || bytes.Length == 0)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    if (!Settings.Global.recieveMessagesFromOthers && !ConnectionBase.Main.Contains(ipAddress))
                    {
                        hElement.GoodResponse = true;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_OK });
                    }

                    if (API.MainServer.OnStartTyping != null && Settings.Global.recieveMessagesFromOthers && ConnectionBase.Main.Contains(ipAddress))
                    {
                        API.MainServer.OnStartTyping.BeginInvoke(API.MainServer, new UserBeginTypingEventArgs(ipAddress), null, null);
                    }

                    hElement.GoodResponse = true;
                    return new MemoryStream(new byte[] { CODE_RESPONSE_OK });
                }

                /// <summary>
                /// Обработчик ответа на запрос
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <returns>Результат, потлученные от сервера</returns>
                static public bool ResponceHandler(Stream bytes)
                {
                    bytes.Position = 0;
                    return bytes != null && bytes.Length > 0 && bytes.ReadByte() == CODE_RESPONSE_OK;
                }

            }

            #endregion

            #region Attachment

            /// <summary>
            /// Обработчик запросов на скачивание файла
            /// </summary>
            static internal class AttachmentHandler
            {
                public const byte CODE_REQUEST = 0;
                public const byte CODE_RESPONSE_ERROR = ActionData.ACTION_CODE_ERROR;

                /// <summary>
                /// Обработчик исходящего запроса
                /// </summary>
                /// <param name="id">Идентификатор</param>
                /// <returns>Поток, содержащий запрос</returns>
                static public Stream SendRequestHandler(string id)
                {
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_REQUEST }, Utility.Text.Encoding.GetBytes(id)));
                }

                /// <summary>
                /// Обработчик принятого запроса
                /// </summary>
                /// <param name="Stream">Принятые данные</param>
                /// <param name="ipAddress">IP-адрес</param>
                /// <returns>Поток, содержащи ответ на запрос</returns>
                static public Stream RecieveHandler(Stream Stream, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_GET_FILE, CODE_REQUEST, false));
                    Stream.Position = 0;
                    if (Stream == null || Stream.Length == 0 || (hElement.RequestSubtype = (byte)Stream.ReadByte()) != CODE_REQUEST)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    if (!Settings.Global.recieveMessagesFromOthers && !ConnectionBase.Main.Contains(ipAddress))
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    byte[] bytes = new byte[Stream.Length];
                    Stream.Read(bytes, 0, bytes.Length);

                    string id = Utility.Text.Encoding.GetString(bytes, 1);

                    Attachment attachment = AttachmentsBase.Main.GetRegisteredAttachmentById(ipAddress, id);

                    if (attachment == null)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }
                    else
                    {
                        attachment.DownloadCount++;
                        hElement.GoodResponse = true;
                        return new FileStream(attachment.Path, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            #endregion

            #region AdditionalBoard text

            /// <summary>
            /// Обработчик запроса текста на дополнительной доске
            /// </summary>
            static internal class AdditionalBoardTextHandler
            {
                public const byte CODE_SEND_REQUEST = 0;
                public const byte CODE_RESPONSE_OK = 1;
                public const byte CODE_RESPONSE_ERROR = 2;

                /// <summary>
                /// Обработчик исходящего запроса
                /// </summary>
                /// <param name="text">Текст на доп. доске</param>
                /// <returns>Поток, содержащий запрос</returns>
                public static Stream SendRequestHandler(string text)
                {
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_SEND_REQUEST }, Utility.Text.Encoding.GetBytes(text)));
                }

                /// <summary>
                /// Обработчикпринятого запроса
                /// </summary>
                /// <param name="stream">Принятые данные</param>
                /// <param name="ipAddress">IP-адрес клиента</param>
                /// <returns>Поток, содержащий ответ на запро</returns>
                public static Stream RecieveHandler(Stream stream, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_SAY_INPUT_TEXT, CODE_SEND_REQUEST, false));
                    stream.Position = 0;
                    if (stream == null || stream.Length == 0 || (hElement.RequestSubtype = (byte)stream.ReadByte()) != CODE_SEND_REQUEST)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    if (!Settings.Global.recieveMessagesFromOthers && !ConnectionBase.Main.Contains(ipAddress))
                    {
                        hElement.GoodResponse = true;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_OK });
                    }

                    byte[] bytes = new byte[stream.Length - 1];
                    stream.Position = 1;
                    stream.Read(bytes, 0, bytes.Length);

                    if (API.MainServer.OnChangeInputText != null && Settings.Global.recieveMessagesFromOthers && ConnectionBase.Main.Contains(ipAddress))
                    {
                        API.MainServer.OnChangeInputText.BeginInvoke(API.MainServer, new TextEventArgs(ipAddress, Utility.Text.Encoding.GetString(bytes, 1)), null, null);
                    }

                    hElement.GoodResponse = true;
                    return new MemoryStream(new byte[] { CODE_RESPONSE_OK });
                }

                /// <summary>
                /// Обработчик ответа на запрос
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <returns>Результат, полученный от сервера</returns>
                public static bool ResponseHandler(Stream bytes)
                {
                    bytes.Position = 0;
                    return bytes != null && bytes.Length > 0 && bytes.ReadByte() == CODE_RESPONSE_OK;
                }
            }

            #endregion

            #region AdditionalBoard canvas
            
            /// <summary>
            /// Обработчик запросов штрихов на допю доске
            /// </summary>
            static internal class AdditionalBoardCanvasHandler
            {
                public const byte CODE_SEND_REQUEST_STROKES = 0;
                public const byte CODE_SEND_REQUEST_SIZE = 1;
                public const byte CODE_SEND_REQUEST_BACKGROUND = 2;
                public const byte CODE_RESPONSE_OK = 200;
                public const byte CODE_RESPONSE_ERROR = 201;

                #region Strokes

                /// <summary>
                /// Обработчик исходящего запроса штрихов
                /// </summary>
                /// <param name="strokeCollection">Коллекция штрихов</param>
                /// <returns>Поток, содержащий запрос</returns>
                static public Stream SendRequestHandler_Strokes(StrokeCollection strokeCollection)
                {
                    StrokeLikeList list = new StrokeLikeList(strokeCollection);

                    return new MemoryStream(Utility.Concat(new byte[] { CODE_SEND_REQUEST_STROKES }, list.ConventToBytes()));
                }

                #endregion

                #region Canvas size

                /// <summary>
                /// Обработчик исходящего запроса размера холста
                /// </summary>
                /// <param name="X">Ширина</param>
                /// <param name="Y">Высота</param>
                /// <returns>Поток, содержащий запрос</returns>
                static public Stream SendRequesrHandler_Size(int X, int Y)
                {
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_SEND_REQUEST_SIZE }, Utility.Convent.GetBytes(X), Utility.Convent.GetBytes(Y)));
                }

                #endregion

                #region Background

                /// <summary>
                /// Обработчик исходящего запроса изменения фонового изображения
                /// </summary>
                /// <param name="imageFile">Массив байт, содержащих файл изображения</param>
                /// <returns>Поток, содержащий запрос</returns>
                public static Stream SendRequestHandler_Background(byte[] imageFile)
                {
                    return new MemoryStream(Utility.Concat(new byte[] { CODE_SEND_REQUEST_BACKGROUND }, imageFile));
                }

                #endregion

                /// <summary>
                /// Обработчик принятого запроса
                /// </summary>
                /// <param name="stream">Полученные данные</param>
                /// <param name="ipAddress">IP-адрес клиента</param>
                /// <returns>Поток, содержащий ответ на запрос</returns>
                static public Stream RecieveHandler(Stream stream, IPAddress ipAddress)
                {
                    var hElement = RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_UPDATE_INCKCANVAS, CODE_RESPONSE_ERROR, false));

                    stream.Position = 0;
                    if (stream == null || stream.Length == 0 || (hElement.RequestSubtype = (byte)stream.ReadByte()) >= 200)
                    {
                        hElement.GoodResponse = false;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                    }

                    if (!Settings.Global.recieveMessagesFromOthers && !ConnectionBase.Main.Contains(ipAddress))
                    {
                        hElement.GoodResponse = true;
                        return new MemoryStream(new byte[] { CODE_RESPONSE_OK });
                    }

                    byte[] bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    switch (bytes[0])
                    {
                        #region Strokes

                        case CODE_SEND_REQUEST_STROKES:

                            var t = new StrokeLikeList();
                            t.ConventFromBytes(bytes, 1);

                            if (API.MainServer.OnChangeInckCanvasDelegate != null)
                            {
                                API.MainServer.OnChangeInckCanvasDelegate.BeginInvoke(API.MainServer, new InckCanvasChangedEventArgs(ipAddress, t.ConventToStrokeCollection()), null, null);
                            }
                            hElement.GoodResponse = true;
                            return new MemoryStream(new byte[] { CODE_RESPONSE_OK });

                        #endregion

                        #region Canvas size

                        case CODE_SEND_REQUEST_SIZE:
                            try
                            {
                                int X = Utility.Convent.ToInt32(bytes, 1);
                                int Y = Utility.Convent.ToInt32(bytes, 5);
                                if (API.MainServer.OnChangeInckCanvasSize != null)
                                {
                                    API.MainServer.OnChangeInckCanvasSize.BeginInvoke(API.MainServer, new IncCanvasSizeChanged(ipAddress, X, Y), null, null);
                                }
                            }
                            catch
                            {
                                hElement.GoodResponse = false;
                                return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                                
                            }
                            hElement.GoodResponse = true;
                            return new MemoryStream(new byte[] { CODE_RESPONSE_OK });;

                        #endregion

                        #region Backgound

                        case CODE_SEND_REQUEST_BACKGROUND:
                            if (API.MainServer.OnChangeIncCanvasBackground != null)
                            {
                                var tt = new byte[bytes.Length - 1];
                                Array.Copy(bytes, 1, tt, 0, tt.Length);
                                API.MainServer.OnChangeIncCanvasBackground.BeginInvoke(API.MainServer, new IncCanvasBackgroundChanged(ipAddress, tt), null, null);
                            }
                            hElement.GoodResponse = true;
                            return new MemoryStream(new byte[] { CODE_RESPONSE_OK });

                        #endregion
                    }

                    hElement.GoodResponse = false;
                    return new MemoryStream(new byte[] { CODE_RESPONSE_ERROR });
                }


                /// <summary>
                /// Обработчик ответа на запрос
                /// </summary>
                /// <param name="bytes">Принятые данные</param>
                /// <returns>Результат, полученный от сервера</returns>
                public static bool ResponseHandler(Stream bytes)
                {
                    bytes.Position = 0;
                    return bytes != null && bytes.Length > 0 && bytes.ReadByte() == CODE_RESPONSE_OK;
                }
            }

            #endregion

        }

        #endregion

        static public API MainServer = null;

        #region Events

        public delegate void UserStartTyping(object sender, UserBeginTypingEventArgs e);
        /// <summary>
        /// Генерируется когда собседник начал вводить сообщение
        /// </summary>
        public event UserStartTyping OnStartTyping;

        public delegate void MessageRecieve(object sender, RecievedMessageEventArgs e);
        /// <summary>
        /// Генерируется, когда собеседник отправил сообщение
        /// </summary>
        public event MessageRecieve OnMessageRecieve;

        public delegate void ChangeInputTextDelegate(object sender, TextEventArgs e);
        /// <summary>
        /// Генерируется, когда собеседник изменил текст на доп. доске
        /// </summary>
        public event ChangeInputTextDelegate OnChangeInputText;

        public delegate void ChangeInckCanvasDelegate(object sender, InckCanvasChangedEventArgs e);
        /// <summary>
        /// Генерируется, когда собеседник отправил штрики на доп. доске
        /// </summary>
        public event ChangeInckCanvasDelegate OnChangeInckCanvasDelegate;

        public delegate void ChangeInckCanvasSizeDelegate(object sender, IncCanvasSizeChanged e);
        /// <summary>
        /// Генерируется, когда собеседник изменил размер холста
        /// </summary>
        public event ChangeInckCanvasSizeDelegate OnChangeInckCanvasSize;

        public delegate void ChangeIncCanvasBackgroundDekegate(object sender, IncCanvasBackgroundChanged e);
        /// <summary>
        /// Генерируется, когда собеседник изсенил фотовое изображения доп. доски
        /// </summary>
        public event ChangeIncCanvasBackgroundDekegate OnChangeIncCanvasBackground;

        #endregion

        NetServer netServer = NetServer.MainServer;
        /// <summary>
        /// Прослушиваемый порт
        /// </summary>
        public UInt16 ServerPort { get { return this.netServer.Port; } }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName
        {
            get { return Settings.Global.UserName; }
            set
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                {
                    Settings.Global.UserName = string.Empty;
                }
                else
                {
                    Settings.Global.UserName = value;
                }
            }
        }

        #region Constructors

        public API(){
            
        }

        #endregion

        #region Server Recieve Methods

        /// <summary>
        /// Включает сервер
        /// </summary>
        public void StartServer()
        {
            this.netServer.OnRecieve += this.OnDataRecieve;
            this.netServer.Start();
        }

        /// <summary>
        /// Выключает сервер
        /// </summary>
        public void StopServer()
        {
            new Thread(new ThreadStart(() =>
            {
                this.netServer.OnRecieve -= this.OnDataRecieve;
                this.netServer.Stop();
            })).Start();
        }

        /// <summary>
        /// Главный (глобальный) обработчик всех запросов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns>Поток, содержащий ответ на запрос</returns>
        Stream OnDataRecieve(object sender, RecievedDataEventArgs e)
        {
            ActionData actionData = ActionData.ConvertFromByte(e.Data);
            Stream result = null;

            switch (actionData.ActionCode)
            {

                #region Message

                case ActionData.ACTION_CODE_MESSAGE:
                    result = API.TypesHandlers.MessageHandler.RecieveHandler(actionData.Data, e.IpAdress);
                    break;
                #endregion

                #region Client Name

                case ActionData.ACTION_CODE_GET_CLIENT_NAME:
                    result = TypesHandlers.ClientNameHandler.ReceiveHandler(actionData.Data, e.IpAdress);
                    break;

#endregion

                #region Client photo

                case ActionData.ACTION_CODE_GET_CLIENT_PHOTO:
                    result = TypesHandlers.ClientPhotoHandler.ReceiveHandler(actionData.Data, e.IpAdress);
                    break;

                #endregion

                #region Typing

                case ActionData.ACTION_CODE_USER_I_AM_TYPING:
                    result = TypesHandlers.TypingHandler.RecieveHandler(actionData.Data, e.IpAdress);
                    break;

#endregion

                #region Attachment

                case ActionData.ACTION_CODE_GET_FILE:
                    result = TypesHandlers.AttachmentHandler.RecieveHandler(actionData.Data, e.IpAdress);
                    break;

#endregion

                #region Input text

                case ActionData.ACTION_CODE_SAY_INPUT_TEXT:
                    result = TypesHandlers.AdditionalBoardTextHandler.RecieveHandler(actionData.Data, e.IpAdress);
                    break;

                #endregion

                #region InkCanvas

                case ActionData.ACTION_CODE_UPDATE_INCKCANVAS:
                    result = TypesHandlers.AdditionalBoardCanvasHandler.RecieveHandler(actionData.Data, e.IpAdress);
                    break;

                #endregion

                default:
                    RequestHistoryBase.Inbox.AddToHistory(new HistoryElement(e.IpAdress, ActionData.ACTION_CODE_ERROR, 0, false));
                   result = new MemoryStream(new byte[] { ActionData.ACTION_NOT_FOUND });
                    break;
            }

            return result;
        }
        #endregion

        #region Send/Get

        #region Send

        /// <summary>
        /// Отправляет запрос
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="actionData">Отправляемые данные</param>
        /// <param name="resultStream">Поток, для записи ответа</param>
        /// <param name="onRecieveProcess">Выполняется во вермя принятия ответа</param>
        public void Send(IPAddress ipAddress, UInt16 port, ActionData actionData, Stream resultStream, Action<object, ProcessEventArgs> onRecieveProcess = null)
        {
            this.netServer.Send(ipAddress, actionData.ConvertToStream(), resultStream, port, onRecieveProcess);
        }

        #endregion

        #region Send Message

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="message">Сообщение</param>
        /// <returns>Результат отправки</returns>
        public bool SendMessage(IPAddress ipAddress, UInt16 port, Message message)
        {
            MemoryStream ms = new MemoryStream();

            var toSend = new ActionData(ActionData.ACTION_CODE_MESSAGE, API.TypesHandlers.MessageHandler.SendHandler(message));
            Send(ipAddress, port, toSend, ms);

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_MESSAGE, TypesHandlers.MessageHandler.CODE_RECEVE_OK, API.TypesHandlers.MessageHandler.ResponseHandler(ms))).GoodResponse;
        }

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="chatConnection">Получатель</param>
        /// <param name="message">Сообщение</param>
        /// <returns>Результат отправки</returns>
        public bool SendMessage(ChatConnection chatConnection, Message message)
        {     
            return SendMessage(chatConnection.IP, chatConnection.Port, message);
        }

        #endregion

        #region Input text

        /// <summary>
        /// Отправляет текст на доп. доске
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="text">Текст</param>
        /// <returns></returns>
        public bool SendInputText(IPAddress ipAddress, UInt16 port, string text)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_SAY_INPUT_TEXT, TypesHandlers.AdditionalBoardTextHandler.SendRequestHandler(text)), ms);

            if (ms == null) return false;

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_SAY_INPUT_TEXT, TypesHandlers.AdditionalBoardTextHandler.CODE_SEND_REQUEST, API.TypesHandlers.AdditionalBoardTextHandler.ResponseHandler(ms))).GoodResponse;
        }

        #endregion

        #region Client's name

        /// <summary>
        /// Отправляет запрос на имя собеседника
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">порт</param>
        /// <returns>Имя собеседника</returns>
        public string GetClientName(IPAddress ipAddress, UInt16 port)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_GET_CLIENT_NAME, TypesHandlers.ClientNameHandler.SendRequestHandler()), ms);

            var s = TypesHandlers.ClientNameHandler.ResponseHandler(ms);
            RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_GET_CLIENT_NAME, TypesHandlers.ClientNameHandler.CODE_SEND_REQUEST, !string.IsNullOrWhiteSpace(s)));
  
            return s;
        }

        #endregion

        #region Client's photo

        /// <summary>
        /// Отправляет запрос на фото собеседника
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <returns>Масив байт, содержащий файл с изображением </returns>
        public byte[] GetClientPhoto(IPAddress ipAddress, UInt16 port)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_GET_CLIENT_PHOTO, TypesHandlers.ClientPhotoHandler.SendRequestHandler()), ms);

            var s = TypesHandlers.ClientPhotoHandler.ResponseHandler(ms);

            RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_GET_CLIENT_PHOTO, TypesHandlers.ClientPhotoHandler.CODE_REQUEST, s != null));


            return s;
        }

        #endregion

        #region Typing

        /// <summary>
        /// Отправляет запрос на уведомление о том, что пользователь начал набирать сообщение
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <returns>Результат отправки</returns>
        public bool SetTyping(IPAddress ipAddress, UInt16 port)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_USER_I_AM_TYPING, TypesHandlers.TypingHandler.SendRequestHandler()), ms);

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_USER_I_AM_TYPING, TypesHandlers.TypingHandler.CODE_REQUEST, API.TypesHandlers.TypingHandler.ResponceHandler(ms))).GoodResponse;
        }

        #endregion

        #region Attachment

        /// <summary>
        /// Скачивает файл
        /// </summary>
        /// <param name="ipAdress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="id">Идентификатор файла</param>
        /// <param name="path">Путь к сохранению файла</param>
        /// <param name="onRecieveProcess">Выполняется во время скачивания файла</param>
        /// <returns>Результат скачивания</returns>
        public bool GetAttachment(IPAddress ipAdress, UInt16 port, string id, string path, Action<object, ProcessEventArgs> onRecieveProcess = null)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            try
            {
                
                Send(ipAdress, port, new ActionData(ActionData.ACTION_CODE_GET_FILE, TypesHandlers.AttachmentHandler.SendRequestHandler(id)), fs, onRecieveProcess);

                fs.Position = 0;
                int first = fs.ReadByte();
                if (fs.Length == 0 || (fs.Length < 3 && (first == ActionData.ACTION_CODE_ERROR || first == ActionData.ACTION_NOT_FOUND)))
                {
                    fs.Close();
                    System.IO.File.Delete(path);
                    RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAdress, TypesHandlers.AttachmentHandler.CODE_REQUEST, 0, false));
                    return false;
                }
                fs.Close();
                RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAdress, ActionData.ACTION_CODE_GET_FILE, TypesHandlers.AttachmentHandler.CODE_REQUEST, true));
                return true;
            }
            catch (Exception e)
            {
                fs.Close();
                throw e;
            }
        }

        #endregion

        #region InckCanvas

        /// <summary>
        /// Отправляет штрихи на допю доске
        /// </summary>
        /// <param name="ipAddress">ip-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="strokeCollection">Коллекция штрихов</param>
        /// <returns>Результат отправки</returns>
        public bool SendInckCanvasStrokes(IPAddress ipAddress, UInt16 port, StrokeCollection strokeCollection)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.SendRequestHandler_Strokes(strokeCollection)), ms);

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_STROKES, TypesHandlers.AdditionalBoardCanvasHandler.ResponseHandler(ms))).GoodResponse;
        }

        /// <summary>
        /// Отправляет размер холста
        /// </summary>
        /// <param name="ipAddress">ip-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="X">Ширина</param>
        /// <param name="Y">Высота</param>
        /// <returns>Результат отправки</returns>
        public bool SendInckCanvasSize(IPAddress ipAddress, UInt16 port, int X, int Y)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.SendRequesrHandler_Size((int)X, (int)Y)), ms);

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_SIZE, TypesHandlers.AdditionalBoardCanvasHandler.ResponseHandler(ms))).GoodResponse;
        }

        /// <summary>
        /// Отпраляет фоновое изображение холста
        /// </summary>
        /// <param name="ipAddress">IP-адрес</param>
        /// <param name="port">Порт</param>
        /// <param name="backgroundImage">Массив байт, содержащий файл изображения</param>
        /// <returns>Результат отправки</returns>
        public bool SendIncCanvasBackgroundImage(IPAddress ipAddress, UInt16 port, byte[] backgroundImage)
        {
            MemoryStream ms = new MemoryStream();
            Send(ipAddress, port, new ActionData(ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.SendRequestHandler_Background(backgroundImage)), ms);

            return RequestHistoryBase.Outbox.AddToHistory(new HistoryElement(ipAddress, ActionData.ACTION_CODE_UPDATE_INCKCANVAS, TypesHandlers.AdditionalBoardCanvasHandler.CODE_SEND_REQUEST_BACKGROUND, TypesHandlers.AdditionalBoardCanvasHandler.ResponseHandler(ms))).GoodResponse;
        }

        #endregion

        #endregion
    }
}
