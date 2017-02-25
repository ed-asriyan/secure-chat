using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chat
{
    /// <summary>
    /// Представляет собой настройки программы
    /// </summary>
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// Глобальные настройки
        /// </summary>
        static public Settings Global = new Settings();

        #region Constructor

        public Settings()
        {
            this.UserName = Environment.UserName;
        }

        #endregion

        #region Connecting & server

        public int RECIEVE_TIMEOUT = 10000;                       // таймаут получения данных
        public int SEND_TIMEOUT = 10000;                          // таймаут отправки данных
        public int RECIEVE_BUFFER_SIZE = 1024;                    // буфер получения данных
        public int SEND_BUFFER_SIZE = 1024;                       // буфер отправки данных

        public const UInt16 ServerPortDefault = 55708;            // порт сервера по умалчанию
        public UInt16 ServerPort = ServerPortDefault;             // порт сервера

        public int EncryptionBlock = 1000000;                     // блок шифрования

        public bool recieveConnectionsFromOthers = true;          // принимать ли соединения от посторонних

        public bool DisableEncryptionWithLocalComputers = false;  // отлючать ли шифрование с компьютерами в локальной сети

        public ushort RSAKeySize = 2048;                          // размер ключа RSA

        public string DefaultSecretKey = null;                    // секретное слово по умолчанию

        public int LongpollSimultaneousThreads = 5;               // кол-во параллельных (одновременных) longpoll-соединений
        public int LongpollResponseTimeout = 5000;                // таймаут ожидания ответа в режиме longpoll
        public int LongpollResponseCheckInterval = 500;           // интервал проверки ответа в базе в режиме longpoll
        public int LongpollEventWaitingInterval = 400;            // интервал проверки новых событий на стороне сервера в режиме longpoll

        #endregion

        #region API

        public string UserName = Environment.UserName;            // имя ползователя

        byte[] _userPhotoBytes = null;                            // фото пользователя
        public byte[] UserPhotoBytes                              // фото пользователя
        {
            get
            {
                return this._userPhotoBytes;
            }
            set
            {
                this._userPhoto = null;
                this._userPhotoBytes = value;
            }
        }

        [NonSerialized]
        BitmapImage _userPhoto = null;                           // фото пользователя
        public BitmapImage UserPhoto                             // фото пользователя
        {
            get
            {
                if (this._userPhoto == null)
                {
                    BitmapImage result = null;
                    App.MainWindow.Dispatcher.Invoke(new Action(() =>
                    {
                        try
                        {
                            result = new BitmapImage();
                            result.BeginInit();
                            result.StreamSource = new MemoryStream(this.UserPhotoBytes);
                            result.EndInit();
                            
                        }
                        catch (Exception ex)
                        {
                            using (MemoryStream memory = new MemoryStream())
                            {
                                Properties.Resources.ProfilePhotoDefault.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                                memory.Position = 0;
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = memory;
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.EndInit();

                                result = bitmapImage;
                            }
                        }
                        this._userPhoto = result;
                    }));
                }
                return this._userPhoto;
            }
        }


        public bool recieveMessagesFromOthers = true;            // получать ли сообщения от посторонних

        #endregion

        #region Files

        public uint MaxPhotosSize = 3;                           // максимальных размер изображения для автоскачивания (МБ)

        public uint MaxFilesSize = 2;                            // максимальных размер файла для автоскачивания (МБ)

        #endregion

        #region UI & Sound

        #region Chatbox background color

        // цвета фона чата
        byte _ChatBoxBackgroundColorALPHA = 255;                 
        byte _ChatBoxBackgroundColorRED = 239;
        byte _ChatBoxBackgroundColorGREEN = 242;
        byte _ChatBoxBackgroundColorBLUE = 242;

        public Color ChatBoxBackgroundColor                      // цвет фона чата
        {
            get
            {
                return Color.FromArgb(this._ChatBoxBackgroundColorALPHA, this._ChatBoxBackgroundColorRED, this._ChatBoxBackgroundColorGREEN, this._ChatBoxBackgroundColorBLUE);
            }
            set
            {
                this._ChatBoxBackgroundColorALPHA = value.A;
                this._ChatBoxBackgroundColorRED = value.R;
                this._ChatBoxBackgroundColorGREEN = value.G;
                this._ChatBoxBackgroundColorBLUE = value.B;
            }
        }

        #endregion

        #region Program background color (disabled)

        // фвета фотка программы
        byte _ProgramBackgroundColorALPHA = 255;
        byte _ProgramBackgroundColorRED = 239;
        byte _ProgramBackgroundColorGREEN = 239;
        byte _ProgramBackgroundColorBLUE = 242;

        public Color ProgramBackgroundColor                     // фвет фона программы
        {
            get
            {
                return Color.FromArgb(this._ProgramBackgroundColorALPHA, this._ProgramBackgroundColorRED, this._ProgramBackgroundColorGREEN, this._ProgramBackgroundColorBLUE);
            }
            set
            {
                this._ProgramBackgroundColorALPHA = value.A;
                this._ProgramBackgroundColorRED = value.R;
                this._ProgramBackgroundColorGREEN = value.G;
                this._ProgramBackgroundColorBLUE = value.B;
            }
        }

        #endregion

        #region Message bubble color

        // фвета фона сообщения
        byte _MessageBackgroundColorALPHA = 255;
        byte _MessageBackgroundColorRED = 255;
        byte _MessageBackgroundColorGREEN = 255;
        byte _MessageBackgroundColorBLUE = 255;

        public Color MessageBackgroundColor                     // фвет фона сообщения
        {
            get
            {
                return Color.FromArgb(this._MessageBackgroundColorALPHA, this._MessageBackgroundColorRED, this._MessageBackgroundColorGREEN, this._MessageBackgroundColorBLUE);
            }
            set
            {
                this._MessageBackgroundColorALPHA = value.A;
                this._MessageBackgroundColorRED = value.R;
                this._MessageBackgroundColorGREEN = value.G;
                this._MessageBackgroundColorBLUE = value.B;
            }
        }

        #endregion

        public bool PlayMessageSound = true;                     // звук при получении сообщения

        public bool ShowPopupNotifications = true;               // показывать ли pop-up уведомления

        public bool ShowTrayIcon = true;                         // показывать ли икону в трее

        public bool AskForExit = true;                           // показывать ли диалоговое окно при закрытии

        public int NotificationShowingTimeSpan = 5000;           // длительность показа pop-up уведомления

        #endregion

        #region Current chats

        public bool SaveConnections = true;                      // сохранять ли диалоги
        public bool SaveMessages = true;                         // сохранять ли сообщения

        public bool UpperCaseForDefault = true;                  // писать ли новые предложения с большой буквы

        #endregion

        public uint OnlineCheckInterval = 35000;                 // интервал проверки собеседника на онлайн

        public uint LocalComputersUpdateInterval = 60000;        // интервал обновления списка локальных компьютеров
    }
}
