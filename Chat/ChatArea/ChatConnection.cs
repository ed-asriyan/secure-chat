using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Соединение с собеседником
    /// </summary>
    [Serializable]
    public class ChatConnection
    {
        #region Private fields

        /// <summary>
        /// Привязка к базам
        /// </summary>
        public bool IsBindedWithBases; // если true, будут изменяться значения в базах, при изменении secretword, longpoll, useonlylongpoll и т.п.

        bool _longPoll = false;
        bool _encryptionIsNeeded = true;

        #endregion

        #region Public values

        /// <summary>
        /// IP-адрес
        /// </summary>
        public IPAddress IP { get; set; }

        /// <summary>
        /// Порт
        /// </summary>
        public UInt16 Port { get; set; }

        /// <summary>
        /// Секретное слово
        /// </summary>
        public string SecretWord
        {
            get
            {
                byte[] t = null;
                if ((t = PasswordsBase.Main.GetSecret(this.IP)) == null)
                {
                    return string.Empty;
                }
                else return Utility.Text.Encoding.GetString(t);
            }
            set
            {
                if (this.IsBindedWithBases)
                {
                    SetSecretKey(value);
                }
            }
        }

        /// <summary>
        /// Режим longpoll
        /// </summary>
        public bool LongPoll
        {
            get { return this._longPoll; }
            set
            {
                if (this.IsBindedWithBases)
                {
                    if (value)
                    {
                        if (this._longPoll != value)
                        {
                            NetServer.MainServer.Longpoll.Client.Add(this.IP, this.Port);

                        }
                    }
                    else
                    {
                        NetServer.MainServer.Longpoll.Client.Remove(this.IP);
                    }
                }
                this._longPoll = value;
            }
        }

        bool _useLongpollOnly = false;

        /// <summary>
        /// Испольщование только longpoll-соединения
        /// </summary>
        public bool UseLongpollOnly
        {
            get { return this._useLongpollOnly; }
            set
            {
                this._useLongpollOnly = value;
                if (this.IsBindedWithBases)
                {
                    if (value)
                    {
                        LongpollOnlyConnectionBase.Global.Add(this.IP);
                    }
                    else
                    {
                        LongpollOnlyConnectionBase.Global.Remove(this.IP);
                    }
                }
            }
        }

        /// <summary>
        /// Необходимость шифрования
        /// </summary>
        public bool EncruptionIsNeeded
        {
            get
            {
                return this._encryptionIsNeeded;
            }
            set
            {
                this._encryptionIsNeeded = value;
                PasswordsBase.Main.SetEncryptionNeed(this.IP, value);
            }
        }

        /// <summary>
        /// Устанавливает секретное слово
        /// </summary>
        /// <param name="key">Секретное слово</param>
        public void SetSecretKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                PasswordsBase.Main.RemoveSecret(this.IP);
            }
            else
            {
                PasswordsBase.Main.SetSecret(this.IP, Utility.Text.Encoding.GetBytes(key));
            }
        }

        #endregion

        #region Constructors

        public ChatConnection(IPAddress ipAdress, UInt16 port, bool isBindedWhithBases = true)
        {
            this.IsBindedWithBases = isBindedWhithBases;
            this.IP = ipAdress;
            this.Name = this.IP.ToString();
            
            this.Port = port;
        }

        #endregion

        #region Destructor

        ~ChatConnection()
        {
            this.LongPoll = false;
        }

        #endregion

        #region Message

        /// <summary>
        /// Создаёт сообщение
        /// </summary>
        /// <param name="messageBody">Текст сообщения</param>
        /// <param name="filePath">Путь к прикреплённому файлу</param>
        /// <returns>Объект <see cref="Message"/></returns>
        public Message CreateMessage(string messageBody, string filePath = "")
        {
            if (string.IsNullOrEmpty(filePath))
            {
                var result = new Message(messageBody);
                result.WasSent = false;
                result.Out = true;
                return result;
            }
            else
            {
                if (!System.IO.File.Exists(filePath)) return null;

                var attach = AttachmentsBase.Main.RegisterAttachment(this.IP, filePath);

                Message message = new Message(API.MainServer.UserName, messageBody, true, attach);
                message.WasSent = false;
                message.Out = true;
                return message;
            }
        }

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns>Результат отправки</returns>
        public bool SendMessage(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");
            return API.MainServer.SendMessage(this.IP, this.Port, message);
        }

        /// <summary>
        /// Создаёт и отправляет сообщение
        /// </summary>
        /// <param name="messageBody">Текст сообщения</param>
        /// <param name="filePath">Путь к файлу</param>
        /// <returns>Результат отправки</returns>
        public Message CreateAndSendMessage(string messageBody, string filePath = "")
        {
            var message = this.CreateMessage(messageBody, filePath);
            if (message == null) return null;

            if (SendMessage(message))
            {
                return message;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Name

        /// <summary>
        /// Имя собеседника
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Обновить имя собеседника
        /// </summary>
        /// <returns>Имя собеседника</returns>
        public string UpdateName()
        {
            string name = API.MainServer.GetClientName(this.IP, this.Port);
            this.Name = name;
            return name;
        }

        #endregion

        #region Photo

        [NonSerialized]
        BitmapImage _profilePhoto = null;

        /// <summary>
        /// Фотография собеседника
        /// </summary>
        public BitmapImage ProfilePhoto
        {
            set
            {
                this._profilePhoto = value;
            }
            get
            {
                if (this._profilePhoto == null)
                {
                    this.ProfilePhoto = new BitmapImage(new Uri("Images/NoProfilePhoto.png", UriKind.RelativeOrAbsolute));
                }
                return this._profilePhoto;
            }
        }

        /// <summary>
        /// Обновляет фото собеседника
        /// </summary>
        /// <returns>Фото</returns>
        public BitmapImage UpdateProfilePhoto()
        {
            byte[] response = new byte[1];
            try
            {
                response = API.MainServer.GetClientPhoto(this.IP, this.Port);
            }
            catch
            {

            }

            App.MainWindow.Dispatcher.Invoke(new Action(() =>
            {
                BitmapImage result = new BitmapImage();
                result.BeginInit();

                try
                {

                    result.StreamSource = new MemoryStream(response);
                    result.EndInit();
                    this.ProfilePhoto = result;

                }
                catch
                {
                    result = new BitmapImage();
                    result.BeginInit();
                    result.UriSource = new Uri("Images/NoProfilePhoto.png", UriKind.RelativeOrAbsolute);
                    result.EndInit();
                    this.ProfilePhoto = result;
                    
                }
            }));
            return this.ProfilePhoto;
        }

        #endregion

        #region Input text on Additional board

        /// <summary>
        /// Отправляет текст на доп. доске
        /// </summary>
        /// <param name="text">Текст</param>
        /// <returns>Результат отправки</returns>
        public bool SendInputText(string text)
        {
            return API.MainServer.SendInputText(this.IP, this.Port, text);
        }

        #endregion

        #region Typing notification

        /// <summary>
        /// Отправляет уведомление о наборе сообщения
        /// </summary>
        /// <returns>Результат отправки</returns>
        public bool SendTypingNotification()
        {
            return API.MainServer.SetTyping(this.IP, this.Port);
        }

        #endregion

        #region Download attachment

        /// <summary>
        /// Скачивает файл
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="downloadPath">Путь к сохранинию</param>
        /// <param name="process">Выполняется во время скачивания</param>
        public void DownloadAttachment(string id, string downloadPath, Action<object, ProcessEventArgs> process = null)
        {
            if (!API.MainServer.GetAttachment(this.IP, this.Port, id,  downloadPath, process))
            {
                throw new Exception("Незозможно скачать файл. Возможно собеседник ограничил доступ к нему, или просто отключился от сети.");
            }
        }

        #endregion

        #region InkCanves strokes

        /// <summary>
        /// Отправляет штрихи на допю доске
        /// </summary>
        /// <param name="collection">Коллекция штрихов</param>
        /// <returns>Результат отправки</returns>
        public bool SendInkCanvasStrokes(System.Windows.Ink.StrokeCollection collection)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            return API.MainServer.SendInckCanvasStrokes(this.IP, this.Port, collection);
        }

        #endregion

        #region InkCanvas background

        /// <summary>
        /// Отправляет фоновое изображение холста
        /// </summary>
        /// <param name="filePath">Путь к изображению</param>
        /// <returns>Результат отправки</returns>
        public bool SendInkCanvasBackground(string filePath)
        {
            byte[] bytes = new byte[1];
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                bytes = System.IO.File.ReadAllBytes(filePath);
            }
            return API.MainServer.SendIncCanvasBackgroundImage(this.IP, this.Port, bytes);
        }

        #endregion

        #region InkCanvas size

        /// <summary>
        /// Отправляет размер холста
        /// </summary>
        /// <param name="X">Ширина</param>
        /// <param name="Y">Высота</param>
        /// <returns>Результат отправки</returns>
        public bool SendInkCanvasSize(int X, int Y)
        {
            if (X < 0) throw new ArgumentOutOfRangeException("X", "Must be positive number");
            if (Y < 0) throw new ArgumentOutOfRangeException("Y", "Must be positive number");

            return API.MainServer.SendInckCanvasSize(this.IP, this.Port, X, Y);
        }

        #endregion
    }
}
