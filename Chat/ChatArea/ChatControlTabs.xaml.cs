using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Net;

namespace Chat
{
    /// <summary>
    /// Interaction logic for ChatControlTabs.xaml
    /// </summary>
    public partial class ChatControlTabs : UserControl
    {
        #region Private values

        List<ChatBox> chatBoxList = new List<ChatBox>(); // список чатов
        List<ChatControlTabHeader> chatBoxHeadersList = new List<ChatControlTabHeader>(); // список хэдэров

        #endregion

        #region Public values

        /// <summary>
        /// Список ChatBox'ов
        /// </summary>
        public List<ChatBox> ChatBoxList { get { return this.chatBoxList; } }

        /// <summary>
        /// Кол-во диалогов
        /// </summary>
        public int ChatBoxCount { get { return this.chatBoxList.Count; } }

        public ChatBox this[int index]
        {
            get
            {
                return index < this.chatBoxList.Count && index > 0 ? this.chatBoxList[index] : null;
            }
        }

        public ChatConnection ChatConnection(int index)
        {
            var chatBox = this[index];
            return chatBox == null ? null : chatBox.ChatConnection;
        }

        #endregion

        #region Constructors

        public ChatControlTabs()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        public void Init()
        {
            API.MainServer.OnMessageRecieve += this.OnMessageRecieve;
            API.MainServer.OnStartTyping += this.OnBeginTyping;
            API.MainServer.OnChangeInputText += this.OnInputTextChanged;
            API.MainServer.OnChangeInckCanvasDelegate += this.OnUpdateInckCanvasStrokes;
            API.MainServer.OnChangeInckCanvasSize += this.OnUpdateIncCanvasSize;
            API.MainServer.OnChangeIncCanvasBackground += this.OnUpdateIncCanvasBackground;

            NetServer.MainServer.OnLongPollConnectionsChanged += this.OnLongpollConnected;
        }

        #endregion

        #region Search chatbox methods

        /// <summary>
        /// Поиск чата по соединению
        /// </summary>
        /// <param name="connection">Объект соединения</param>
        /// <returns>Найденный элемент</returns>
        public ChatBox Find(ChatConnection connection)
        {
            return Find(connection.IP);
        }

        /// <summary>
        /// Поиск чата по IP
        /// </summary>
        /// <param name="ipAdress">IP</param>
        /// <returns>Найденный элемент</returns>
        public ChatBox Find(IPAddress ipAdress)
        {
            ChatBox chatBox = null;
            this.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        string ip = ipAdress.ToString();
                        chatBox = this.chatBoxList.First(x => x.ChatConnection.IP.ToString() == ip);
                    }
                    catch { }

                });
            return chatBox;
        }

        #endregion

        #region Add chatbox methods

        /// <summary>
        /// Добавляет чат в список чатов
        /// </summary>
        /// <param name="chatBox">Чат</param>
        /// <param name="setOnTop">Показать поверх остальных</param>
        public void AddChatBox(ChatBox chatBox, bool setOnTop)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.chatBoxHeadersList.Add(chatBox.ChatControlTabHeader);
                Headers.Children.Add(chatBox.ChatControlTabHeader);

                this.GridChatBoxes.Children.Add(chatBox);
                this.chatBoxList.Add(chatBox);

                ConnectionBase.Main.Add(chatBox.ChatConnection.IP);

                App.MainWindow.GetStartedImage.Visibility = System.Windows.Visibility.Collapsed;

                if (this.chatBoxList.Count == 1 || setOnTop)
                {
                    SetOnTop(chatBox);
                    BeginShowSearchBox();
                }
                

            }));
        }

        /// <summary>
        /// Создаёт и добавляет чат в список чатов
        /// </summary>
        /// <param name="connection">Объект соединения</param>
        /// <param name="setOnTop">Показать поверх остальных</param>
        /// <returns>Чат</returns>
        public ChatBox AddChatBox(ChatConnection connection, bool setOnTop)
        {
            ChatBox chatBox = null;
            this.Dispatcher.Invoke(() =>
            {
                chatBox = Find(connection);
                if (chatBox == null)
                {
                    chatBox = new ChatBox(connection);
                    chatBox.Visibility = System.Windows.Visibility.Collapsed;
                    this.AddChatBox(chatBox, setOnTop);
                }
            });
            return chatBox;
        }

        /// <summary>
        /// Создаёт и добавляет чат в список чатов
        /// </summary>
        /// <param name="ipAdress">IP</param>
        /// <param name="port">Порт</param>
        /// <param name="setOnTop">Показать поверх остальных</param>
        /// <returns>Чат</returns>
        public ChatBox AddChatBox(IPAddress ipAdress, UInt16 port, bool setOnTop)
        {
            ChatBox chatBox = null;
            chatBox = Find(ipAdress);
            
            
            if (chatBox == null)
            {
                var cc = new ChatConnection(ipAdress, port, false);
                chatBox = this.AddChatBox(cc, setOnTop);
            }
            return chatBox;
        }

        #endregion

        #region Remove chatbox methods

        /// <summary>
        /// Удаляет чат из списка
        /// </summary>
        /// <param name="chatBox">Чат</param>
        public void RemoveChatBox(ChatBox chatBox)
        {
            if (this.chatBoxList.Contains(chatBox))
            {
                chatBox.Terminate();
                if (chatBox.ChatControlTabHeader != null)
                {
                    if (this.chatBoxHeadersList.Contains(chatBox.ChatControlTabHeader))
                    {
                        this.chatBoxHeadersList.Remove(chatBox.ChatControlTabHeader);
                    }
                    if (this.Headers.Children.Contains(chatBox.ChatControlTabHeader))
                    {
                        this.Headers.Children.Remove(chatBox.ChatControlTabHeader);
                    }
                }

                this.chatBoxList.Remove(chatBox);

                ConnectionBase.Main.Remove(chatBox.ChatConnection.IP);
                AttachmentsBase.Main.UnregisterAllAttachments(chatBox.ChatConnection.IP);
                PasswordsBase.Main.RemoveSecret(chatBox.ChatConnection.IP);
                PasswordsBase.Main.SetEncryptionNeed(chatBox.ChatConnection.IP, true);

                this.GridChatBoxes.Children.Remove(chatBox);
                if (this.chatBoxList.Count > 0)
                {
                    this.SetOnTop(this.chatBoxList[this.chatBoxList.Count - 1]);
                }
                else
                {
                    
                }

                if (this.chatBoxList.Count == 0)
                {
                    BeginHideSearchBox();
                    App.MainWindow.UpdateTitle(null);
                }
            }
        }

        /// <summary>
        /// Удаляет чат
        /// </summary>
        /// <param name="chatConnection">Объект соединения</param>
        public void RemoveChatBox(ChatConnection chatConnection)
        {
            var chatBox = this.Find(chatConnection);

            if (chatBox != null)
            {
                this.RemoveChatBox(chatBox);
            }
        }

        /// <summary>
        /// Удаляет чат
        /// </summary>
        /// <param name="ipAddress">IP</param>
        public void RemoveChatBox(IPAddress ipAddress)
        {
            var chatBox = this.Find(ipAddress);

            if (chatBox != null)
            {
                this.RemoveChatBox(chatBox);
            }
        }

        #endregion

        #region Events handlers

        #region Message recieve

        void OnMessageRecieve(object sender, RecievedMessageEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatBox chatBox = AddChatBox(e.Sender, Settings.Global.ServerPort, false);
                chatBox.AddMessage(e.Message);

                if (chatBox.Visibility != System.Windows.Visibility.Visible) chatBox.ChatControlTabHeader.SetReadState(false);

                if (chatBox.ChatControlTabHeader != null) chatBox.ChatControlTabHeader.Update(e.Message);

                if (App.MainWindow.WindowState == WindowState.Minimized && Settings.Global.ShowPopupNotifications)
                {
                    NewMessageNotificationWindow nw = new NewMessageNotificationWindow(); nw.Init(e.Message);
                    nw.Show();
                    App.MainWindow.Focus();
                }

            }));
        }

        #endregion

        #region Typing

        void OnBeginTyping(object sender, UserBeginTypingEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                ChatBox chatBox = Find(e.IpAddress);
                if (chatBox != null) chatBox.SetTyping(true);
            }));
        }

        #endregion

        #region Longpoll connection status

        void OnLongpollConnected(object sender, LongpollConnectEventArgs e)
        {
            ChatBox chatbox = Find(e.IpAddress);
            if (chatbox != null)
            {
                chatbox.SetLongpollConnectionStatus((uint)e.ConnectionsCount);
            }
        }

        #endregion

        #region Input text board

        void OnInputTextChanged(object sender, TextEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ChatBox chatBox = Find(e.ipAddress);
                if (chatBox != null)
                {
                    chatBox.SetAdditionInputText(e.text);
                }
            });
        }

        #endregion

        #region Update InckCanvas

        void OnUpdateInckCanvasStrokes(object sender, InckCanvasChangedEventArgs e)
        {
            ChatBox chatBax = Find(e.ipAddress);

            if (chatBax == null) return;

            chatBax.SetCanvasStrokes(e.inckCanvasBytes);
        }

        void OnUpdateIncCanvasSize(object sender, IncCanvasSizeChanged e)
        {
            ChatBox chatBox = Find(e.ipAddress);

            if (chatBox == null) return;

            chatBox.SetCanvasSize((int)e.X, (int)e.Y);
        }

        void OnUpdateIncCanvasBackground(object sender, IncCanvasBackgroundChanged e)
        {
            ChatBox cb = Find(e.ipAddress);

            if (cb == null) return;

            cb.SetCanvasBackground(e.bitmap);
        }

        #endregion

        #endregion

        #region Chatbox header

        /// <summary>
        /// Показывает чат поверх остальных
        /// </summary>
        /// <param name="chatBox"></param>
        /// <returns>Результат выполнения</returns>
        public bool SetOnTop(ChatBox chatBox)
        {
            if (Find(chatBox.ChatConnection) == null) return false;

            this.Dispatcher.Invoke(() =>
            {
                BeginShowChatBoxOnTop(chatBox);
                foreach (var c in chatBoxList)
                {
                    if (c.ChatConnection.IP.ToString() != chatBox.ChatConnection.IP.ToString())
                    {
                        c.ChatControlTabHeader.IsSelected = false;
                    }
                }
                chatBox.ChatControlTabHeader.IsSelected = true;
                chatBox.ChatControlTabHeader.SetReadState(true);
                App.MainWindow.UpdateTitle(chatBox.ChatConnection);
            });
            return true;
        }

        #endregion

        #region Animation

        void BeginShowSearchBox()
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(350);

            this.Dispatcher.Invoke(() =>
            {
                DoubleAnimation anim = new DoubleAnimation(0.8, duration);

                this.txtbxSearchIcon.Visibility = Visibility.Visible;
                this.txtbxSearch.Visibility = System.Windows.Visibility.Visible;
                this.txtbxBorder.Visibility = System.Windows.Visibility.Visible;

                this.txtbxSearch.BeginAnimation(OpacityProperty, anim);
                this.txtbxSearchIcon.BeginAnimation(OpacityProperty, anim);
                this.txtbxBorder.BeginAnimation(OpacityProperty, anim);
            });
        }

        void BeginHideSearchBox()
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(350);

            this.Dispatcher.Invoke(() =>
            {
                DoubleAnimation anim = new DoubleAnimation(0, duration);

                anim.Completed += (object sender, EventArgs e) =>
                {
                    this.txtbxSearchIcon.Visibility = System.Windows.Visibility.Collapsed;
                    this.txtbxSearch.Visibility = System.Windows.Visibility.Collapsed;
                    this.txtbxBorder.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.txtbxSearch.BeginAnimation(OpacityProperty, anim);
                this.txtbxSearchIcon.BeginAnimation(OpacityProperty, anim);
                this.txtbxBorder.BeginAnimation(OpacityProperty, anim);
            });
        }

        void BeginShowChatBoxOnTop(ChatBox chatBox)
        {
            if (chatBox.Visibility == Visibility.Visible) return;

            TimeSpan duration = TimeSpan.FromMilliseconds(150);

            var visibles = this.chatBoxList.Where(x => x.Visibility == System.Windows.Visibility.Visible).ToArray();

            this.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < visibles.Length; i++ )
                {
                    DoubleAnimation animHide = new DoubleAnimation(0, duration);
                    animHide.DecelerationRatio = 0.5;
                    int j = i;
                    animHide.Completed += (object sender, EventArgs e) =>
                    {
                        visibles[j].Visibility = Visibility.Collapsed;
                    };

                    visibles[i].BeginAnimation(OpacityProperty, animHide);
                }

                DoubleAnimation animShow = new DoubleAnimation(1, duration);

                chatBox.Visibility = System.Windows.Visibility.Visible;

                chatBox.BeginAnimation(OpacityProperty, animShow);
            });
        }

        #endregion

        #region Searchbox

        private void txtbxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < this.Headers.Children.Count; i++)
            {
                if (string.IsNullOrEmpty(this.txtbxSearch.Text) || (this.Headers.Children[i] as ChatControlTabHeader).Head.Text.ToLower().Contains(this.txtbxSearch.Text.ToLower()))
                {
                    (this.Headers.Children[i] as ChatControlTabHeader).Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    (this.Headers.Children[i] as ChatControlTabHeader).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}
