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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows.Ink;

using System.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for ChatBox.xaml
    /// </summary>
    public partial class ChatBox : UserControl
    {
        long lastTyping = 0;

        ChatConnection chatConnection = null;

        Thread gettingName = null;
        bool _gettingNameAlive = true;

        Thread showUserTypingThread = null;

        /// <summary>
        /// Список сообщений (<see cref="MessageControl"/>)
        /// </summary>
        List<MessageControl> messages = new List<MessageControl>();

        /// <summary>
        /// Выделенные сообщения (<see cref="MessageControl"/>)
        /// </summary>
        List<MessageControl> selectedMessages = new List<MessageControl>();

        string currentFilePath = "";

        #region Values

        /// <summary>
        /// Заголовок чата
        /// </summary>
        public string HeaderText
        {
            get
            {
                string result = string.Empty;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    result = this.ConnectionName.Content as string;
                }));
                return result;
            }
            set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ConnectionName.Content = value;
                }));
            }
        }

        /// <summary>
        /// Фото собеседника
        /// </summary>
        public BitmapImage HeaderPhoto
        {
            get
            {
                BitmapImage result = null;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    result = (this.ConnectionPhoto.Source as BitmapImage);
                }));
                return result;
            }
            set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ConnectionPhoto.Source = value;

                    for (int i = 0; i < this.messages.Count && i < 50; i++)
                    {
                        if (!this.messages[i].message.Out)
                        {
                            this.messages[i].ProfilePhoto.Source = this.ChatConnection.ProfilePhoto;
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// Кол-во сообщений
        /// </summary>
        public int MessagesCount
        {
            get
            {
                int result = 0;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    result = this.messages.Count;
                }));
                return result;
            }
        }

        /// <summary>
        /// Последнее сообщение
        /// </summary>
        public Message LastMessage
        {
            get
            {
                Message result = null;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (this.messages.Count == 0)
                    {
                        result = null;
                        return;
                    }
                    result = ((this.messages[this.messages.Count - 1])).message;
                }));
                return result;
            }
        }

        /// <summary>
        /// Путь к текущему прикреплённому файлу
        /// </summary>
        public string CurrentFilePath
        {
            get { return currentFilePath; }
            set
            {
                currentFilePath = value;

                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.ButtonFileName.Text = string.Empty;

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.btmimgAttachment.Source = new BitmapImage(new Uri("Images/FileSelect.png", UriKind.RelativeOrAbsolute));
                            this.AttachmentButton.ToolTip = "Прикрепить файл";
                            this.btnDeleteAttachment.Visibility = System.Windows.Visibility.Collapsed;
                        }));
                    }
                    else
                    {
                        if (!System.IO.File.Exists(value)) throw new Exception("Не удаётся найти файл");

                        this.ButtonFileName.Text = System.IO.Path.GetExtension(value);
                        this.AttachmentButton.ToolTip = value;
                        this.btnDeleteAttachment.Visibility = System.Windows.Visibility.Visible;

                        using (System.Drawing.Icon sysicon = System.Drawing.Icon.ExtractAssociatedIcon(value))
                        {
                            this.btmimgAttachment.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                      sysicon.Handle,
                                      System.Windows.Int32Rect.Empty,
                                      System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(16, 16));
                        }
                    }
                }));
            }
        }

        /// <summary>
        /// Текущий введённый текст
        /// </summary>
        public string CurrentInputText
        {
            get
            {
                string result = "";
                this.Dispatcher.Invoke(new Action(() =>
                {
                    result = this.MessageInput.Text;
                }));
                return result;
            }
            private set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.MessageInput.Text = value;
                }));
            }
        }

        /// <summary>
        /// Массив из всех сообщений
        /// </summary>
        public Message[] Messages
        {
            get
            {
                return this.messages.Select(x => x.message).ToArray();
            }
        }

        /// <summary>
        /// Массив из выделенных сообщений
        /// </summary>
        public MessageControl[] SelectedMessages
        {
            get
            {
                return this.selectedMessages.ToArray();
            }
        }

        /// <summary>
        /// Прикреплённый <see cref="ChatCollection"/>
        /// </summary>
        public ChatConnection ChatConnection
        {
            get { return this.chatConnection; }
            private set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.ConnectionName.Content = value.Name;
                    this.ConnectionAdress.Content = value.IP.ToString();
                    this.chbxLongpoll.IsChecked = value.LongPoll;
                }));
                this.chatConnection = value;
            }
        }

        /// <summary>
        /// Кисть фона
        /// </summary>
        public Brush BackgroundBrush { get { return this.MessagesArea.Background; } set { this.MessagesArea.Background = value; } }

        /// <summary>
        /// Прикреплённый <see cref="ChatControlTabHeader"/>
        /// </summary>
        public ChatControlTabHeader ChatControlTabHeader { get; set; }

        /// <summary>
        /// Кол-во сообщений
        /// </summary>
        public int MessCount { get { return this.MessagesArea.Children.Count; } }

        #endregion

        #region Constructors

        public ChatBox(ChatConnection chatConnection)
        {
            InitializeComponent();
            Init(chatConnection);

            this.btnDeleteSelected.ToolTip = "Удалить";
            this.btnSelectAll.ToolTip = "Выделить все сообщения";
            this.btnUnselectAll.ToolTip = "Отменить выделение";
            this.txblLongPoll.Text = "Подключён в режиме longpoll";
            this.btnAddBoardSwitcher.ToolTip = "Нажмите, чтобы открыть дополнительную доску";
            this.txblTextAddBoarddescription.Text = "Текст собеседника:";
            this.chbxLongpoll.Content = "Longpoll";
            this.chbxLongpoll.ToolTip = "Подключиться к собеседнику в режиме longpoll";
            this.AdditionalBoard.Visibility = System.Windows.Visibility.Collapsed;
            this.chbxLongpoll.IsChecked = chatConnection.LongPoll;
            this.SetCanvasSize(500, 500);
            this.addbdCanvasSetColor.Fill = new SolidColorBrush(this.addbrInkCanvas.DefaultDrawingAttributes.Color);
            this.MessagesAreaViewer.Background = new SolidColorBrush(Settings.Global.ChatBoxBackgroundColor);

            this._chanvasStrokeUpdateDetectorAlive = true;
            chanvasStrokeUpdateDetector = new Thread(new ThreadStart(() =>
            {
                while (this._chanvasStrokeUpdateDetectorAlive)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        if (this.addbrInkCanvas.Strokes.Count != this._canvasStrokesCount)
                        {
                            if (OnIncCanvasStrokesCountChanged != null) OnIncCanvasStrokesCountChanged();
                        }
                        this._canvasStrokesCount = this.addbrInkCanvas.Strokes.Count;
                    }));

                    Thread.Sleep(1000);
                }
            })); chanvasStrokeUpdateDetector.Start();

            this.OnIncCanvasStrokesCountChanged += SendCanvas;

        }

        /// <summary>
        /// Завершает все процессы в ChatBox'е
        /// </summary>
        public void Terminate()
        {
            this._chanvasStrokeUpdateDetectorAlive = false;
            this._gettingNameAlive = false;
            NetServer.MainServer.Longpoll.Client.Remove(this.chatConnection.IP);
            this.chatConnection.SecretWord = string.Empty;
            this.ChatConnection = null;
        }

        #endregion

        #region Destructor

        ~ChatBox()
        {
            Terminate();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="chatConnection"><see cref="ChatConnection"/>, к которому будет прикреплён ChatBox</param>
        public void Init(ChatConnection chatConnection)
        {
            if (chatConnection.IsBindedWithBases) throw new ArgumentException("Parameter must be unbinned", "chatConnection");

            this.ChatConnection = new ChatConnection(chatConnection.IP, chatConnection.Port, true);

            this.ChatConnection.SecretWord = chatConnection.SecretWord;
            this.ChatConnection.LongPoll = chatConnection.LongPoll;
            this.ChatConnection.UseLongpollOnly = chatConnection.UseLongpollOnly;
            this.ChatConnection.EncruptionIsNeeded = chatConnection.EncruptionIsNeeded;

            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ConnectionName.Content = this.chatConnection.Name;
                this.ConnectionPhoto.Source = this.ChatConnection.ProfilePhoto;
                this.ConnectionAdress.Content = chatConnection.IP.ToString();
                this.chbxLongpoll.IsChecked = this.chatConnection.LongPoll;

                this.LongpollArea.Visibility = Visibility.Visible;
                this.btnConnectionParameters.Visibility = Visibility.Visible;
                this.btnAddBoardSwitcher.Visibility = Visibility.Visible;

                SetLongpollConnectionStatus((uint)NetServer.MainServer.Longpoll.Server.LongpollConnectionCount(this.ChatConnection.IP));
            }));

            this.ChatControlTabHeader = new ChatControlTabHeader();
            this.ChatControlTabHeader.Init(this);

            this.ChatControlTabHeader.HeadText = this.chatConnection.Name;

            this.gettingName = new Thread(new ThreadStart(new Action(() =>
            {
                while (this._gettingNameAlive)
                {
                    try
                    {
                        this.ChatConnection.UpdateName();
                        this.HeaderText = this.ChatConnection.Name;
                        this.SetOnlineStatus(true);

                        this.HeaderPhoto = this.ChatConnection.UpdateProfilePhoto();


                        this.ChatControlTabHeader.Update();
                    }
                    catch (Exception ex)
                    {
                        this.SetOnlineStatus(false);
                    }

                    Thread.Sleep((int)Settings.Global.OnlineCheckInterval);
                }
            }))); this.gettingName.Start();



        }

        #endregion

        #region Messages

        #region Move message control from sending to sent

        /// <summary>
        /// Перемещение <see cref="MessageControl"/>'а из отправляемых в отправленные
        /// </summary>
        /// <param name="messageControl">Сообщение</param>
        public void MoveMessageControlFromSendingToSent(MessageControl messageControl)
        {
            if (messageControl == null) throw new ArgumentNullException("messageControl");

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!this.SendingMessages.Children.Contains(messageControl)) return;// throw new Exception("StackPanel \"" + this.SendingMessages.Name + "\" do not contains this messageControl");
                if (this.MessagesArea.Children.Contains(messageControl)) return;// throw new Exception("StackPanel \"" + this.MessagesArea.Name + "\" already contains this messageControl");
                if (this.messages.Contains(messageControl)) return;// throw new Exception("MessageControl list already contains this messageControl");

                this.SendingMessages.Children.Remove(messageControl);

                messageControl.Time.Text = DateTime.Now.ToShortTimeString();
                messageControl.ShowTimeIcon();

                this.MessagesArea.Children.Add(messageControl);

                this.messages.Add(messageControl);

            /*    new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(5000);
                    this.Dispatcher.Invoke(() => messageControl.Delete());
                })).Start(); */

            }));

        }

        #endregion

        #region Add messagecontrol to list

        /// <summary>
        /// Добавляет сообщение в чат
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <returns><see cref="MessageControl"/></returns>
        public MessageControl AddMessage(Message message)
        {
            message.chatConnetcion = this.chatConnection;

            MessageControl mess = null;

            this.Dispatcher.Invoke(new Action(() =>
            {
                bool scrollToBottomIsNeeded = this.MessagesAreaViewer.ScrollableHeight == this.MessagesAreaViewer.VerticalOffset;

                mess = new MessageControl();

                mess.Init(message, this);

                if (message.WasSent && message.Out) // если исходящее, уже отправленное
                {
                    
                    this.messages.Add(mess);
                    this.MessagesArea.Children.Add(mess);
                 //   this.MessagesAreaViewer.ScrollToBottom();
                    if (scrollToBottomIsNeeded) this.MessagesAreaViewer.ScrollToBottom();
               /*     new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(5000);
                        this.Dispatcher.Invoke(() => mess.Delete());
                    })).Start();  */

                }
                else if (message.WasSent && !message.Out) // если принятое
                {
                    this.messages.Add(mess);
                    mess.BeginAnimationShow();
                    this.MessagesArea.Children.Add(mess);                    

                    if (scrollToBottomIsNeeded) this.MessagesAreaViewer.ScrollToBottom();

                /*    new Thread(new ThreadStart(() =>
                    {
                        Thread.Sleep(5000);
                        this.Dispatcher.Invoke(() => mess.Delete());
                    })).Start(); */
                }
                else if (!message.WasSent && message.Out) // если исходящее, но ещё не отправленное
                {
                    mess.BeginAnimationShow();
                    this.SendingMessages.Children.Add(mess);

                    mess.OnSendAgain += (object sender, EventArgs e) =>
                    {
                        this.SendLastInQuerry();
                    };
                 
                    if (scrollToBottomIsNeeded) this.MessagesAreaViewer.ScrollToBottom();
                }

                mess.OnSelect += this.OnMessageSelect;
                mess.OnUnselect += this.OnMessageUnselect;
                mess.OnMessageDelete += this.OnMessageDelete;
            }));

            if (!message.Out)
            {
                this.ChangeChatName(message.AutorName);

                if (this.Visibility != System.Windows.Visibility.Visible ||
                    App.MainWindow.WindowState == WindowState.Minimized
                    ) PlayMewMessageNotificationSound();

                SetTyping(false);
                SetOnlineStatus(true);
                this.ChatControlTabHeader.Update();
            }


            return mess;
        }

        #endregion

        #region Clickable methods

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            this._lastKeyWasBackspace = e.Key == Key.Back;

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
            {
                var caretIndex = this.MessageInput.CaretIndex;
                this.MessageInput.Text = this.MessageInput.Text.Insert(caretIndex, Environment.NewLine);
                this.MessageInput.CaretIndex = caretIndex + 1;
            }
            else if (e.Key == Key.Enter)
            {

                if (this.AdditionalBoard.Visibility != System.Windows.Visibility.Visible)
                {
                    if (!this.MessageSend.IsEnabled) return;
                    MessageSend_Click(null, null);
                    this.lastTyping = 0;
                }
                else
                {
                    SendInputText();
                }
            }
        }

        #endregion

        #region Send message

        bool _isSendingQuerry = false; // нахотятся ли в очереди неотправленные сообщения

        /// <summary>
        /// Оправка сообщений, которые стоят в очереди
        /// </summary>
        void SendLastInQuerry()
        {
            if (_isSendingQuerry) return;

            new Thread(new ThreadStart(() =>
            {
                this._isSendingQuerry = true;

                bool check = false;
                this.Dispatcher.Invoke(new Action(() => check = this.SendingMessages.Children.Count > 0));

                while (check)
                {

                    MessageControl messageControl = null;
                    Message message = null;

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        messageControl = (this.SendingMessages.Children[0] as MessageControl);
                        message = messageControl.message;
                    }));

                    try
                    {
                        messageControl.HideError();
                        messageControl.ShowLoadingIcon();
                        if (message.Send())
                        {
                            message.WasSent = true;
                            try
                            {
                                MoveMessageControlFromSendingToSent(messageControl);
                            }
                            catch { }
                            this.ChatControlTabHeader.Update();
                        }
                        else
                        {
                            throw new Exception("Произошла ошибка во время отправки сообщения");
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            messageControl.ShowError(ex.Message);
                            messageControl.ShowErrorIcon();
                        }));

                        this._isSendingQuerry = false;
                        break;
                    }


                    this.Dispatcher.Invoke(new Action(() => check = this.SendingMessages.Children.Count > 0));
                }

                this._isSendingQuerry = false;
            })).Start();
        }

        private void MessageSend_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.MessageInput.Text) || !string.IsNullOrEmpty(this.CurrentFilePath))
            {
                if (!string.IsNullOrEmpty(this.currentFilePath) && !System.IO.File.Exists(this.currentFilePath)) throw new Exception("Файл " + this.currentFilePath + " не найден");

                var mess = this.chatConnection.CreateMessage(this.CurrentInputText, CurrentFilePath);

                var mc = this.AddMessage(mess);
                mc.ShowLoadingIcon();

                SendLastInQuerry();

                this.CurrentInputText = string.Empty;
                this.CurrentFilePath = string.Empty;

            }
        }

        #region Select message

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var mC in this.messages)
            {
                mC.IsSelected = true;
            }
        }

        private void btnUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = this.selectedMessages.Count - 1; i >= 0; i--)
            {
                this.selectedMessages[i].IsSelected = false;
            }
        }

        void OnMessageSelect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.selectedMessages.Add(sender as MessageControl);

                BeginShowMessagesSelectButtons();
            }));
        }

        void OnMessageUnselect(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.selectedMessages.Remove(sender as MessageControl);

                if (this.selectedMessages.Count == 0) BeginHideMessagesSelectButtons();
            }));
        }

        #endregion

        #region Delete message

        void OnMessageDelete(object sender, EventArgs e)
        {
            this.messages.Remove(sender as MessageControl);

            this.selectedMessages.Remove(sender as MessageControl);

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this.MessagesArea.Children.Contains(sender as MessageControl))
                {
                    this.MessagesArea.Children.Remove(sender as MessageControl);
                }
                else if (this.SendingMessages.Children.Contains(sender as MessageControl))
                {
                    this.SendingMessages.Children.Remove(sender as MessageControl);
                }
            }));

            SendLastInQuerry();

            this.ChatControlTabHeader.Update();
        }

        void DeleteMessages(params MessageControl[] messages)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var m in messages)
                {
                    this.messages.Remove(m);
                    this.selectedMessages.Remove(m);
                    m.Delete();
                }
                if (selectedMessages.Count == 0) BeginHideMessagesSelectButtons();
                this.ChatControlTabHeader.Update();
            }));

        }

        private void btnDeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            DeleteMessages(this.selectedMessages.ToArray());
            if (selectedMessages.Count == 0) BeginHideMessagesSelectButtons();
        }

        #endregion

        #endregion

        #endregion

        #region Chat name & photo

        /// <summary>
        /// Изменяет название чата
        /// </summary>
        /// <param name="newName">Новое название</param>
        public void ChangeChatName(string newName)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ConnectionName.Content = newName;
            }));

            this.ChatControlTabHeader.Update();
        }

        private void ConnectionPhoto_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string savePath = AttachmentsBase.Main.GetPathOfIP(this.ChatConnection.IP) + @"\Фото профиля";

            if (!System.IO.Directory.Exists(savePath)) System.IO.Directory.CreateDirectory(savePath);

            var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder, or whichever encoder you want
            encoder.Frames.Add(BitmapFrame.Create(this.ChatConnection.ProfilePhoto));

            savePath += @"\photo.png";

            try
            {
                var fs = new FileStream(savePath, FileMode.Create);
                encoder.Save(fs);
                fs.Close();
                System.Diagnostics.Process.Start(savePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Online status

        /// <summary>
        /// Устанавливает онлайн статус
        /// </summary>
        /// <param name="online">Онлайн статус</param>
        public void SetOnlineStatus(bool online)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ConnectionOffline.Content = online ? "онлайн" : "оффлайн";
            }));
        }

        #endregion

        #region Typing status

        const int SEND_TYPING_NOTIFICATION_INTERVAL = 7500;

        /// <summary>
        /// Показывает уведомление о том, что пользоваетль начал набирать сообщение
        /// </summary>
        /// <param name="isTyping">Набилает ли пользователь сообщение</param>
        public void SetTyping(bool isTyping)
        {
            if (showUserTypingThread != null) showUserTypingThread.Abort();

            this.ChatControlTabHeader.SetTyping(true);

            showUserTypingThread = new Thread(new ThreadStart(() =>
            {
                if (isTyping)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.Typing.Text = this.chatConnection.Name + " набирает сообщение...";
                        DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                        this.Typing.BeginAnimation(OpacityProperty, animation);
                    }));
                    Thread.Sleep(SEND_TYPING_NOTIFICATION_INTERVAL);
                }
                this.Dispatcher.Invoke(new Action(() =>
                {
                    DoubleAnimation animaion = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                    this.Typing.BeginAnimation(OpacityProperty, animaion);

                    var lMess = this.LastMessage;
                    this.ChatControlTabHeader.SetTyping(false);
                }));
            }));
            showUserTypingThread.Start();
        }

        bool _lastKeyWasBackspace = false; // говорил о том, что полседней нажатой клавишой был backspace
        Thread sendTypingNotificationThread = null;
        private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            #region Auto upper case

            if (!_lastKeyWasBackspace && Settings.Global.UpperCaseForDefault)
            {
                if (this.MessageInput.Text.Length == 0) return;
                int caret = this.MessageInput.CaretIndex;
                char[] text = this.MessageInput.Text.ToCharArray();
                text[0] = char.ToUpper(text[0]);
                for (int i = caret - 2; i < caret; i++)
                {
                    for (int j = i - 1; j > 0 && text[j] == ' '; j--)
                    {
                        if (text[j - 1] == '.' || text[j - 1] == '?' || text[j - 1] == '!')
                        {
                            text[i] = char.ToUpper(text[i]);
                        }
                    }
                }
                this.MessageInput.Text = new string(text);
                this.MessageInput.CaretIndex = caret;

            }
            #endregion

            if (this.AdditionalBoard.Visibility == System.Windows.Visibility.Visible)
            {
                SendInputText();
            }
            else if (this.chbxLongpoll.Visibility == Visibility.Visible)
            {
                if (this.MessageInput.Text.Length == 0) return;
                Thread th = null;
                th = new Thread(new ThreadStart(new Action(() =>
                 {
                     if (sendTypingNotificationThread != null && sendTypingNotificationThread.IsAlive) return;
                     this.sendTypingNotificationThread = th;

                     long now = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                     if (now - lastTyping > SEND_TYPING_NOTIFICATION_INTERVAL / 1000 - 1)
                     {

                         try
                         {
                             this.ChatConnection.SendTypingNotification();
                         }
                         catch { };

                         this.lastTyping = now;

                     }
                 }))); th.Start();
            }
        }

        #endregion

        #region Input text

        /// <summary>
        /// Устанавливает текст собеседника на доп. доске
        /// </summary>
        /// <param name="txt"></param>
        public void SetAdditionInputText(string txt)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.txblTextAddBoard.Text = txt;
            }));
        }

        #endregion

        #region Attachments

        private void AttachmentsListViewer_Click(object sender, RoutedEventArgs e)
        {
            AttachmentsViewer av = new AttachmentsViewer();
            av.Init(this.chatConnection.IP);
            av.ShowDialog();
        }
        private void btnDeleteAttachment_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentFilePath = string.Empty;
        }
        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".*";
            dlg.Filter = "Все файлы (*.*)|*.*";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                try
                {
                    CurrentFilePath = dlg.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void AttachmentButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files.Length > 0)
                {
                    try
                    {
                        this.CurrentFilePath = files[0];
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        #endregion

        #region Additional board

        #region Input text

        const int ADD_INUT_TEXT_UPDATE_INTERVAL = 1;
        int lastAddText = 0;
        Thread sendInputTextThread = null;

        /// <summary>
        /// Отправляет введённый текст
        /// </summary>
        /// <param name="text">Текст</param>
        void SendInputText(string text = null)
        {
            if (text == null) this.Dispatcher.Invoke(new Action(() => text = this.MessageInput.Text));

            Thread t = null;
            t = new Thread(new ThreadStart(() =>
            {

                int now = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                lastAddText = now;

                if (sendInputTextThread != null && sendInputTextThread.IsAlive) sendInputTextThread.Abort();
                sendInputTextThread = t;

                Thread.Sleep(ADD_INUT_TEXT_UPDATE_INTERVAL * 1000);

                if (now == lastAddText)
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        this.adbdTextLoadingIcon.Brush = Brushes.Blue;
                    }));
                    BeginHideAddTextErrorIconAnimation();
                    BeginShowAddBoardTextLoadingAnimation();
                    try
                    {
                        this.ChatConnection.SendInputText(text);
                    }
                    catch (Exception ex)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.adbdTextErrorIcon.ToolTip = "Произошла ошибка при передачи текста. Нажмите, чтобы отправить его ещё раз." + Environment.NewLine + "Описание: " + ex.Message;

                            BeginShowAddBoardTextErrorIconAnimation();
                            return;
                        }));
                    }
                    BeginHideAddBoardTextLoadingAnimation();
                }
            })); t.Start();
        }

        private void adbdTextErrorIcon_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                SendInputText(null);
            })).Start();
        }

        #endregion

        #region InkCanvas

        #region Click events


        private void addbdCanvasZoomIn_Click(object sender, RoutedEventArgs e)
        {
            BeginZoomCanvas(0.1);
        }

        private void addbdCanvasZoomOut_Click(object sender, RoutedEventArgs e)
        {
            BeginZoomCanvas(-0.1);
        }

        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //BeginZoomCanvas(e.Delta / 1000);
        }

        private void addbdBrushSizeIncr_Click(object sender, RoutedEventArgs e)
        {
            this.addbrInkCanvas.DefaultDrawingAttributes.Width++;
            this.addbrInkCanvas.DefaultDrawingAttributes.Height++;
        }

        private void addbdBrushSizeDecr_Click(object sender, RoutedEventArgs e)
        {
            if (this.addbrInkCanvas.DefaultDrawingAttributes.Width > 1 && this.addbrInkCanvas.DefaultDrawingAttributes.Height > 1)
            {
                this.addbrInkCanvas.DefaultDrawingAttributes.Width--;
                this.addbrInkCanvas.DefaultDrawingAttributes.Height--;
            }
        }

        private void addbdCanvasSetColor_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();

            cd.AllowFullOpen = true;
            cd.ShowHelp = true;

            cd.Color = System.Drawing.Color.FromArgb(this.addbrInkCanvas.DefaultDrawingAttributes.Color.A, this.addbrInkCanvas.DefaultDrawingAttributes.Color.R, this.addbrInkCanvas.DefaultDrawingAttributes.Color.G, this.addbrInkCanvas.DefaultDrawingAttributes.Color.B);

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.addbrInkCanvas.DefaultDrawingAttributes.Color = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
                this.addbdCanvasSetColor.Fill = new SolidColorBrush(this.addbrInkCanvas.DefaultDrawingAttributes.Color);
            }
        }

        private void addbdCanvasSetColor_Click(object sender, EventArgs e)
        {

        }

        private void addbdbtnDeleteCanvasBackground_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bg = new byte[] { 0 };

                this.SetCanvasBackground(bg);

                Thread t = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        BeginHideAddBoardCanvasErrorIconAnimation();
                        BeginShowAddBoardCanvasLoadingAnimation();
                        this.ChatConnection.SendInkCanvasBackground(null);
                        BeginHideAddBoardCanvasLoadingAnimation();
                    }
                    catch (Exception ex)
                    {
                        this.adbdCanvasErrorIcon.ToolTip = "Произошла ошибка при передаче фонового изображения." + Environment.NewLine + "Описание: " + ex.Message;
                        BeginShowAddBoardCanvasErrorIconAnimation();
                        BeginHideAddBoardCanvasLoadingAnimation();
                    }
                })); t.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void addbdckbxCanvasBackgroundFill_Unchecked(object sender, RoutedEventArgs e)
        {
            this.addbrinkCanBackgroundImage.Height = this.addbrinkCanBackgroundImage.Source.Height;
            this.addbrinkCanBackgroundImage.Width = this.addbrinkCanBackgroundImage.Source.Width;
        }

        private void addbdckbxCanvasBackgroundFill_Checked(object sender, RoutedEventArgs e)
        {
            this.addbrinkCanBackgroundImage.Height = this.addbrInkCanvas.Height;
            this.addbrinkCanBackgroundImage.Width = this.addbrInkCanvas.Width;
        }

        private void addbdbtnVanvasSizeSend_Click(object sender, EventArgs e)
        {
            try
            {
                SetCanvasSize(int.Parse(this.addbdtxtbxCanvasSizeWidth.Text), int.Parse(this.addbdtxtbxCanvasSizeHeight.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            int X = (int)this.addbrInkCanvas.Width;
            int Y = (int)this.addbrInkCanvas.Height;

            Thread t = new Thread(new ThreadStart(() =>
            {
                try
                {
                    this.ChatConnection.SendInkCanvasSize(X, Y);
                }
                catch { }
            })); t.Start();

        }

        private void addbdtxtbxCanvasSizeHeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) addbdbtnVanvasSizeSend_Click(sender, e);
        }

        private void addbdbtnChangeCanvasBackground_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog od = new System.Windows.Forms.OpenFileDialog();

            od.Filter = "Изображения (*.png, *.jpg, *.jpeg, *.bmp)|*.bmp;*.jpg;*.jpeg;*png";

            if (od.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var bg = System.IO.File.ReadAllBytes(od.FileName);

                    /*     int quality = this.addbrInkCanvas.Width > this.addbrInkCanvas.Height ? (int)(this.addbrInkCanvas.Height / this.addbrInkCanvas.Width * 100) : (int)(this.addbrInkCanvas.Width / this.addbrInkCanvas.Height * 100);

                         System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(new MemoryStream(bg));


                         MemoryStream ms = new MemoryStream();

                         System.Drawing.Imaging.ImageCodecInfo jpgEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders()
                             .Where(codec => codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid).Single();
                         var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                         var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                         encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(qualityEncoder, quality);
                         bitmap.Save(ms, jpgEncoder, encoderParams);

                         bg = ms.ToArray();
                     * */

                    this.SetCanvasBackground(bg);

                    Thread t = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                            BeginHideAddBoardCanvasErrorIconAnimation();
                            BeginShowAddBoardCanvasLoadingAnimation();
                            this.ChatConnection.SendInkCanvasBackground(od.FileName);
                            BeginHideAddBoardCanvasLoadingAnimation();
                        }
                        catch (Exception ex)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.adbdCanvasErrorIcon.ToolTip = "Произошла ошибка при передачи фона." + Environment.NewLine + "Описание: " + ex.Message;
                                BeginShowAddBoardCanvasErrorIconAnimation();
                            }));
                            return;
                        }
                    })); t.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.Filter = "Изображение в формате PNG | *.png";
            saveDialog.Title = "Сохранение изображения холста";
            saveDialog.DefaultExt = ".png";
            saveDialog.FileName = this.chatConnection.Name + " " + DateTime.Now.ToString().Replace(':', '-') + ".png";
            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // double zk = (this.addbdCanvasGrid.Height < this.addbdCanvasGrid.Width ? this.addbdCanvasGrid.Width / this.addbdCanvasViewbox.ActualWidth : this.addbdCanvasGrid.Height / this.addbdCanvasViewbox.ActualHeight);
                new Thread(new ThreadStart(() =>
                {
                    double t = this._InkCanvasGridScaleTransform;

                    this.BeginZoomCanvas(0, 1);
                    Thread.Sleep(400);
                    try
                    {


                        MemoryStream screenshot = null;
                        try
                        {
                            screenshot = Utility.UI.GetScreenshot(this.addbdCanvasGrid);
                            if (screenshot == null) throw new Exception("Не удалось захватить изображение.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Произошла ошибка." + Environment.NewLine + "Описание: " + ex.Message);
                            return;
                        }

                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            System.IO.File.WriteAllBytes(saveDialog.FileName, screenshot.ToArray());
                        }));

                        //    BeginZoomCanvas(0, zoomBefore);

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Произошла ошибка." + Environment.NewLine + "Описание: " + ex.Message);
                    }
                    Thread.Sleep(300);

                    this.BeginZoomCanvas(0, t);

                })).Start();
            }
        }

        private void addbrInkCanvas_StrokeErased(object sender, RoutedEventArgs e)
        {
            SendCanvas();
        }
        private void addbrInkCanvas_SelectionMoved(object sender, EventArgs e)
        {
            SendCanvas();
        }

        private void addbrInkCanvas_SelectionResized(object sender, EventArgs e)
        {
            SendCanvas();
        }

        private void adbdPenMode_Click(object sender, RoutedEventArgs e)
        {
            this.addbrInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            BeginHightlightPenTool();
        }

        private void adbdSelectMode_Click(object sender, RoutedEventArgs e)
        {
            this.addbrInkCanvas.EditingMode = InkCanvasEditingMode.Select;
            BeginHightlightSelectTool();
        }

        private void adbdRubbishMode_Click(object sender, RoutedEventArgs e)
        {
            this.addbrInkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            BeginHightlightRubTool();
        }

        #endregion

        delegate void OnIncCanvasStrokesCountChangedHandler();
        event OnIncCanvasStrokesCountChangedHandler OnIncCanvasStrokesCountChanged;

        Thread chanvasStrokeUpdateDetector = null;
        bool _chanvasStrokeUpdateDetectorAlive = true;
        int _canvasStrokesCount = 0;

        private void addbrInkCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.addbrInkCanvas.EditingMode == InkCanvasEditingMode.Select)
            {
                this.addbrInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            }
            else
            {
                this.addbrInkCanvas.EditingMode = InkCanvasEditingMode.Select;
                //   this.addbrInkCanvas.Cursor = Utility.UI.CursorH.CreateCursor(new BitmapImage(new Uri("Images/PenTyping.png", UriKind.RelativeOrAbsolute)), 5, 5);
            }
            // addbrInkCanvas.DefaultDrawingAttributes.Width
        }

        private byte[] InckCanvasBitmapBytes()
        {
            //get the dimensions of the ink control
            int margin = (int)this.addbrInkCanvas.Margin.Left;
            int width = (int)this.addbrInkCanvas.ActualWidth - margin;
            int height = (int)this.addbrInkCanvas.ActualHeight - margin;
            //render ink to bitmap
            RenderTargetBitmap rtb =
            new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(addbrInkCanvas);
            //save the ink to a memory stream
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            byte[] bitmapBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                //get the bitmap bytes from the memory stream
                ms.Position = 0;
                bitmapBytes = ms.ToArray();
            }
            return bitmapBytes;
        }

        /// <summary>
        /// Устанавливает текущие штрихи на холсте
        /// </summary>
        /// <param name="strokes">Коллекция строк</param>
        /// <param name="showNotification">Показывать уведомление</param>
        public void SetCanvasStrokes(StrokeCollection strokes, bool showNotification = true)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.OnIncCanvasStrokesCountChanged -= SendCanvas;

                this.addbrInkCanvas.Strokes = strokes;
                this._canvasStrokesCount = strokes.Count;

                this.OnIncCanvasStrokesCountChanged += SendCanvas;
                //          this.addbrInkCanvas.StrokesReplaced += addbrInkCanvas_StrokesReplaced;
                //          this.addbrInkCanvas.StrokeErased += addbrInkCanvas_StrokeErased;

                if (this.AdditionalBoard.Visibility != System.Windows.Visibility.Visible)
                {
                    this.btnAddBoardSwitcher.ToolTip = "Нажмите, чтобы открыть и увидеть изменения на дополнительной доске";
                    if (showNotification) BeginAutoRotateSwitcher();
                }
            }));
        }

        /// <summary>
        /// Устанавливает размер холста
        /// </summary>
        /// <param name="X">Ширина</param>
        /// <param name="Y">Высота</param>
        public void SetCanvasSize(int X, int Y)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.addbdtxblCanvasSizeInfo.Text = X + " x " + Y;

                this.addbrInkCanvas.Height = Y;
                this.addbrInkCanvas.Width = X;

                this.addbdtxtbxCanvasSizeHeight.Text = Y.ToString();
                this.addbdtxtbxCanvasSizeWidth.Text = X.ToString();
            }));
        }

        /// <summary>
        /// Устанавливает фон холста
        /// </summary>
        /// <param name="imageFile">Массив байт, содержащий файл изображения</param>
        public void SetCanvasBackground(byte[] imageFile)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(imageFile);
                    bi.EndInit();

                    this.SetCanvasSize(bi.PixelWidth, bi.PixelHeight);

                    this.addbrinkCanBackgroundImage.Source = bi;
                }
                catch
                {

                    this.addbrinkCanBackgroundImage.Source = null;
                    return;

                }
            }));
        }

        const int ADD_CANVAS_UPDATE_INTERVAL = 2;
        int lastAddCanvas = 0;
        Thread sendCanvasThread = null;

        /// <summary>
        /// Отправляет штрихи
        /// </summary>
        public void SendCanvas()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                var sc = this.addbrInkCanvas.Strokes.Clone();

                Thread t = null;
                t = new Thread(new ThreadStart(() =>
                {
                    int now = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    lastAddCanvas = now;

                    if (sendCanvasThread != null && sendCanvasThread.IsAlive) sendCanvasThread.Abort();
                    sendCanvasThread = t;

                    Thread.Sleep(ADD_CANVAS_UPDATE_INTERVAL * 1000);

                    if (now == lastAddCanvas)
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.adbdCanvasLoadingIcon.Brush = Brushes.Blue;
                        }));
                        BeginShowAddBoardCanvasLoadingAnimation();
                        try
                        {
                            this.ChatConnection.SendInkCanvasStrokes(sc);
                        }
                        catch (Exception ex)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                this.adbdCanvasErrorIcon.ToolTip = "Произошла ошибка при передачи изображения. Нажмите, чтобы отправить его ещё раз." + Environment.NewLine + "Описание: " + ex.Message;

                                BeginShowAddBoardCanvasErrorIconAnimation();
                                return;
                            }));
                        }
                        BeginHideAddBoardCanvasLoadingAnimation();
                    }
                })); t.Start();
            }));
        }

        private void adbdCanvasErrorIcon_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(SendCanvas)).Start();
        }


        #endregion

        #region Switcher

        private void btnAddBoardSwitcher_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.AdditionalBoard.Visibility == Visibility.Visible)
            {
                BegionAnimationSpinAddBoardSwitcher(180, 359);
            }
            else
            {
                BegionAnimationSpinAddBoardSwitcher(0, 180);
            }
        }

        private void btnAddBoardSwitcher_MouseLeave(object sender, MouseEventArgs e)
        {
            if (this.AdditionalBoard.Visibility == Visibility.Visible)
            {
                BegionAnimationSpinAddBoardSwitcher(0, 180);
            }
            else
            {
                BegionAnimationSpinAddBoardSwitcher(180, 359);
            }
        }

        private void btnAddBoardSwitcher_Click(object sender, RoutedEventArgs e)
        {
            StopAutoRotateSwitcher();
            if (this.AdditionalBoard.Visibility == System.Windows.Visibility.Visible)
            {
                this.AdditionalBoard.Visibility = System.Windows.Visibility.Collapsed;
                BegionAnimationSpinAddBoardSwitcher(180, 359);
                BeginShowAddBoardBottomPanelTransfornationBack();
                this.btnAddBoardSwitcher.ToolTip = "Нажмите, чтобы " + "открыть" + " дополнительную доску";
                new Thread(new ThreadStart(() =>
                {
                    SendInputText(string.Empty);
                })).Start();
            }
            else
            {
                this.AdditionalBoard.Visibility = System.Windows.Visibility.Visible;
                BegionAnimationSpinAddBoardSwitcher(0, 180);
                BeginShowAddBoardBottomPanelTransfornationTo();
                this.btnAddBoardSwitcher.ToolTip = "Нажмите, чтобы " + "скрыть" + " дополнительную доску";
                new Thread(new ThreadStart(() =>
                {
                    SendInputText();
                })).Start();
            }
        }

        #endregion

        #endregion

        #region Animation

        #region Send Message button

        void BeginShowMessSendLoadionAmination()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.griSendMessageLoading.Dispatcher.Invoke(new Action(() =>
            {
                this.MessageSend.IsEnabled = false;
                this.griSendMessageLoading.Visibility = Visibility.Visible;
            }));
            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(0, faddingDuration);

                this.MessageSend.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        void BeginHideMessLoadingAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                this.MessageSend.IsEnabled = true;
                DoubleAnimation animFade = new DoubleAnimation(1, faddingDuration);
                animFade.Completed += delegate(object sender, EventArgs e)
                {
                    this.griSendMessageLoading.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.MessageSend.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        #endregion

        #region Selection Messages buttons

        void BeginShowMessagesSelectButtons()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this.stackSelectedMessagesButtons.IsEnabled) return;

                this.stackSelectedMessagesButtons.IsEnabled = true;

                DoubleAnimation animOpacity = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
                this.stackSelectedMessagesButtons.BeginAnimation(OpacityProperty, animOpacity);
            }));
        }

        void BeginHideMessagesSelectButtons()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!this.stackSelectedMessagesButtons.IsEnabled) return;

                DoubleAnimation animOpacity = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
                animOpacity.Completed += delegate(object s, EventArgs ea)
                {
                    this.stackSelectedMessagesButtons.IsEnabled = false;
                };
                this.stackSelectedMessagesButtons.BeginAnimation(OpacityProperty, animOpacity);
            }));
        }

        #endregion

        #region AdditionalBoard

        #region Text

        void BeginShowAddBoardBottomPanelTransfornationTo()
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(200);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animLeftColumn = new DoubleAnimation(this.AttachmentButton.ActualHeight, 0, duration);
                DoubleAnimation animRightColumnAddBd = new DoubleAnimation(1, duration);
                DoubleAnimation animRightClumnSendMess = new DoubleAnimation(0, duration);

                this.addbdSendTextArea.Visibility = System.Windows.Visibility.Visible;
                this.AttachmentButton.BeginAnimation(Button.WidthProperty, animLeftColumn);
                this.addbdSendTextArea.BeginAnimation(OpacityProperty, animRightColumnAddBd);
                this.SendMessageButtonArea.BeginAnimation(OpacityProperty, animRightClumnSendMess);
            }));
        }

        void BeginShowAddBoardBottomPanelTransfornationBack()
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(200);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animLeftColumn = new DoubleAnimation(this.AttachmentButton.ActualHeight, 37, duration);
                DoubleAnimation animRightColumnAddBd = new DoubleAnimation(0, duration);
                DoubleAnimation animRightClumnSendMess = new DoubleAnimation(1, duration);

                animRightColumnAddBd.Completed += (object sender, EventArgs e) =>
                {
                    this.addbdSendTextArea.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.AttachmentButton.BeginAnimation(Button.WidthProperty, animLeftColumn);
                this.addbdSendTextArea.BeginAnimation(OpacityProperty, animRightColumnAddBd);
                this.SendMessageButtonArea.BeginAnimation(OpacityProperty, animRightClumnSendMess);
            }));
        }

        void BeginShowAddBoardTextLoadingAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(0, 1, faddingDuration);
                DoubleAnimation animInfo = new DoubleAnimation(1, 0, faddingDuration);

                DoubleAnimation animFadeError = new DoubleAnimation(1, 0, faddingDuration);
                animFadeError.Completed += delegate(object sender, EventArgs e)
                {
                    this.adbdTextErrorIcon.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.adbdTextErrorIcon.BeginAnimation(OpacityProperty, animFadeError);

                animInfo.Completed += (object sender, EventArgs e) =>
                {
                    this.adbdTextDescription.Visibility = System.Windows.Visibility.Collapsed;
                };


                if (this.adbdTextErrorIcon.Visibility == System.Windows.Visibility.Visible && this.adbdTextErrorIcon.Opacity == 1)
                {
                    BeginShowAddBoardTextErrorIconAnimation();
                }
                this.adbdTextLoadingIcon.Visibility = Visibility.Visible;
                this.adbdTextLoadingIcon.BeginAnimation(OpacityProperty, animFade);
                this.adbdTextDescription.BeginAnimation(OpacityProperty, animInfo);
            }));
        }

        void BeginHideAddBoardTextLoadingAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(1, 0, faddingDuration);
                DoubleAnimation animInfo = new DoubleAnimation(0, 1, faddingDuration);

                animFade.Completed += delegate(object sender, EventArgs e)
                {
                    this.adbdTextLoadingIcon.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.adbdTextDescription.Visibility = System.Windows.Visibility.Visible;
                this.adbdTextDescription.BeginAnimation(OpacityProperty, animInfo);
                this.adbdTextLoadingIcon.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        void BeginShowAddBoardTextErrorIconAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animShow = new DoubleAnimation(0, 1, faddingDuration);

                if (this.adbdTextLoadingIcon.Visibility == Visibility.Visible && this.adbdTextLoadingIcon.Opacity == 1)
                {
                    BeginHideAddBoardTextLoadingAnimation();
                }

                this.adbdTextErrorIcon.Visibility = Visibility.Visible;
                this.adbdTextErrorIcon.BeginAnimation(OpacityProperty, animShow);
            }));
        }

        void BeginHideAddTextErrorIconAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(1, 0, faddingDuration);
                animFade.Completed += delegate(object sender, EventArgs e)
                {
                    this.adbdTextErrorIcon.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.adbdTextErrorIcon.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        #endregion

        #region Canvas

        #region Draw board

        double _InkCanvasGridScaleTransform = 1;
        void BeginZoomCanvas(double by, double? to = null)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (this._InkCanvasGridScaleTransform <= 0.2 && by < 0 && to == null) return;

                TimeSpan duration = TimeSpan.FromMilliseconds(250);
                double toValueL = to == null ? _InkCanvasGridScaleTransform + by : to.Value;

                ScaleTransform transLocal = new ScaleTransform();

                this.addbdCanvasGrid.LayoutTransform = transLocal;

                // if you use the same animation for X & Y you don't need anim1, anim2 
                DoubleAnimation animLocal = new DoubleAnimation(_InkCanvasGridScaleTransform, toValueL, duration);

                animLocal.DecelerationRatio = 0.8;

                transLocal.BeginAnimation(ScaleTransform.ScaleXProperty, animLocal);
                transLocal.BeginAnimation(ScaleTransform.ScaleYProperty, animLocal);

                this._InkCanvasGridScaleTransform = toValueL;

                this.addbdCanvasLayoutScale.Text = this._InkCanvasGridScaleTransform.ToString();
            }));
        }

        void BeginShowAddBoardCanvasLoadingAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(0, 1, faddingDuration);

                this.adbdCanvasLoadingIcon.Visibility = Visibility.Visible;

                if (this.adbdCanvasErrorIcon.Visibility == System.Windows.Visibility.Visible && this.adbdCanvasErrorIcon.Opacity == 1)
                {
                    BeginHideAddBoardCanvasErrorIconAnimation();
                }

                this.adbdCanvasLoadingIcon.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        void BeginHideAddBoardCanvasLoadingAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(1, 0, faddingDuration);
                animFade.Completed += delegate(object sender, EventArgs e)
                {
                    this.adbdCanvasLoadingIcon.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.adbdCanvasLoadingIcon.BeginAnimation(OpacityProperty, animFade);
            }));
        }


        void BeginShowAddBoardCanvasErrorIconAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animShow = new DoubleAnimation(0, 1, faddingDuration);

                if (this.adbdCanvasLoadingIcon.Visibility == Visibility.Visible && this.adbdCanvasLoadingIcon.Opacity == 1)
                {
                    BeginHideAddBoardCanvasLoadingAnimation();
                }

                this.adbdCanvasErrorIcon.Visibility = Visibility.Visible;
                this.adbdCanvasErrorIcon.BeginAnimation(OpacityProperty, animShow);
            }));
        }

        void BeginHideAddBoardCanvasErrorIconAnimation()
        {
            TimeSpan faddingDuration = TimeSpan.FromMilliseconds(150);

            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation animFade = new DoubleAnimation(1, 0, faddingDuration);
                animFade.Completed += delegate(object sender, EventArgs e)
                {
                    this.adbdCanvasErrorIcon.Visibility = System.Windows.Visibility.Collapsed;
                };

                this.adbdCanvasErrorIcon.BeginAnimation(OpacityProperty, animFade);
            }));
        }

        #endregion

        #region Tools board

        void BeginHightlightPenTool()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(100);

                DoubleAnimation animPen = new DoubleAnimation(0.7, duration);
                DoubleAnimation animSel = new DoubleAnimation(1, duration);
                DoubleAnimation animRub = new DoubleAnimation(1, duration);

                this.adbdPenMode.BeginAnimation(OpacityProperty, animPen);
                this.adbdSelectMode.BeginAnimation(OpacityProperty, animSel);
                this.adbdRubbishMode.BeginAnimation(OpacityProperty, animRub);
            }));
        }

        void BeginHightlightRubTool()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(100);

                DoubleAnimation animPen = new DoubleAnimation(1, duration);
                DoubleAnimation animSel = new DoubleAnimation(1, duration);
                DoubleAnimation animRub = new DoubleAnimation(0.7, duration);

                this.adbdPenMode.BeginAnimation(OpacityProperty, animPen);
                this.adbdSelectMode.BeginAnimation(OpacityProperty, animSel);
                this.adbdRubbishMode.BeginAnimation(OpacityProperty, animRub);
            }));
        }

        void BeginHightlightSelectTool()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(100);

                DoubleAnimation animPen = new DoubleAnimation(1, duration);
                DoubleAnimation animSel = new DoubleAnimation(0.7, duration);
                DoubleAnimation animRub = new DoubleAnimation(1, duration);

                this.adbdPenMode.BeginAnimation(OpacityProperty, animPen);
                this.adbdSelectMode.BeginAnimation(OpacityProperty, animSel);
                this.adbdRubbishMode.BeginAnimation(OpacityProperty, animRub);
            }));
        }

        #endregion

        #endregion

        #region Switcher

        void BegionAnimationSpinAddBoardSwitcher(double from = 0, double to = 180)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(200));

                RotateTransform rt = new RotateTransform();
                this.btnAddBoardSwitcherIcon.RenderTransform = rt;
                this.btnAddBoardSwitcherIcon.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                this.btnAddBoardSwitcherIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
            }));
        }

        void BegionAnimationMoveAddBoardSwitcher(double from = -5, double to = -12)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                DoubleAnimation anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(500));

                // this.grbtnAddionBoardSwitcher.

                this.btnAddBoardSwitcherIcon.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, anim);
            }));
        }

        bool _switcherIsTurning = false; // если тру, то свитчер должен крутиться

        void BeginAutoRotateSwitcher()
        {
            this._switcherIsTurning = true;

            this.Dispatcher.Invoke(new Action(() =>
            {
                ColorAnimation anim = new ColorAnimation(Colors.Yellow, TimeSpan.FromMilliseconds(200));

                SolidColorBrush cb = new SolidColorBrush();
                cb.Color = this.grbtnAddionBoardSwitcher.Background == Brushes.Yellow ? Colors.Yellow : Colors.White;
                this.grbtnAddionBoardSwitcher.Background = cb;
                cb.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }));

            Thread t = new Thread(new ThreadStart(() =>
            {
                while (_switcherIsTurning)
                {
                    BegionAnimationSpinAddBoardSwitcher(0, 180);
                    if (!_switcherIsTurning) return;
                    Thread.Sleep(750);
                    BegionAnimationSpinAddBoardSwitcher(180, 359);
                }
            })); t.Start();
        }

        void StopAutoRotateSwitcher()
        {
            this._switcherIsTurning = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                ColorAnimation anim = new ColorAnimation(Colors.White, TimeSpan.FromMilliseconds(200));

                SolidColorBrush cb = new SolidColorBrush();
                cb.Color = this.grbtnAddionBoardSwitcher.Background == Brushes.Yellow ? Colors.Yellow : Colors.White;
                this.grbtnAddionBoardSwitcher.Background = cb;
                cb.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }));
        }

        #endregion

        #endregion

        #endregion

        #region Longpoll

        private void chbxLongpoll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.chatConnection.LongPoll = true;
            }
            catch { }
        }

        private void chbxLongpoll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.chatConnection.LongPoll = false;
        }

        /// <summary>
        /// Устанавливает longpoll статус
        /// </summary>
        /// <param name="longpollConnectionsCount">Кол-во параллельных лонгпул соединений</param>
        public void SetLongpollConnectionStatus(uint longpollConnectionsCount)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(100);
                if (longpollConnectionsCount != 0)
                {
                    this.txblLongPoll.Text = "Подключён в режиме longpoll (" + longpollConnectionsCount.ToString() + ")";
                    this.chbxLongpoll.Visibility = System.Windows.Visibility.Collapsed;
                    this.txblLongPoll.Visibility = Visibility.Visible;
                }
                else
                {
                    this.txblLongPoll.Visibility = System.Windows.Visibility.Collapsed;
                    this.chbxLongpoll.Visibility = Visibility.Visible;
                }
            }));
        }

        #endregion

        #region Parameters

        private void btnConnectionParameters_Click(object sender, RoutedEventArgs e)
        {
            AddConnectionWindow aw = new AddConnectionWindow();
            aw.Title = "Изменить подключение";
            aw.ConnectionIP.IsEnabled = false;

            aw.ConnectionPort.Text = this.ChatConnection.Port.ToString();
            aw.ConnectionKey.Text = this.ChatConnection.SecretWord;
            aw.LongPoll.IsChecked = this.ChatConnection.LongPoll;
            aw.Encryption.IsChecked = PasswordsBase.Main.EncryptionIsNeed(this.ChatConnection.IP);
            aw.UseLOngpollOnly.IsChecked = this.ChatConnection.UseLongpollOnly;

            bool encryptionIsNeededBefore = this.ChatConnection.EncruptionIsNeeded;
            string secretWordBefore = this.ChatConnection.SecretWord;

            aw.ConnectionIP.Text = this.ChatConnection.IP.ToString(); // айпи надо в самом конце!!!

            bool? result = aw.ShowDialog();
            if (result != null && result.Value)
            {
                /*   if (aw.ChatConnection.EncruptionIsNeeded != encryptionIsNeededBefore)
                   {
                       this.ChatConnection.EncruptionIsNeeded = encryptionIsNeededBefore;
                       new Thread(new ThreadStart(() =>
                       {
                           try
                           {
                               API.MainServer.SendNotificationEncryptionModeChanged(this.ChatConnection, !encryptionIsNeededBefore);
                           }
                           catch { }
                           this.chatConnection.EncruptionIsNeeded = !encryptionIsNeededBefore;
                       })).Start();
                   }

                   if (aw.ChatConnection.SecretWord != secretWordBefore)
                   {
                       string newSW = this.chatConnection.SecretWord;
                       this.ChatConnection.SecretWord = secretWordBefore;
                       new Thread(new ThreadStart(() =>
                       {
                           try
                           {
                               API.MainServer.SendNotificationSecretWordChanged(this.ChatConnection);
                           }
                           catch { }
                           this.chatConnection.SecretWord = newSW;
                       })).Start();
                   } */
                this.ChatConnection.IsBindedWithBases = true;
                this.ChatConnection.IP = aw.ChatConnection.IP;
                this.ChatConnection.Port = aw.ChatConnection.Port;
                this.ChatConnection.UseLongpollOnly = aw.ChatConnection.UseLongpollOnly;
                this.ChatConnection.SecretWord = aw.ChatConnection.SecretWord;
                this.ChatConnection.EncruptionIsNeeded = aw.ChatConnection.EncruptionIsNeeded;
                this.ChatConnection.LongPoll = aw.ChatConnection.LongPoll;

                this.chbxLongpoll.IsChecked = this.ChatConnection.LongPoll;
            }
        }

        #endregion

        #region NotificationSound

        void PlayMewMessageNotificationSound()
        {
            if (!Settings.Global.PlayMessageSound) return;

            new Thread(new ThreadStart(() =>
            {
                System.Media.SoundPlayer sp = new System.Media.SoundPlayer();
                sp.Stream = Properties.Resources.groupon_chime;

                try
                {
                    sp.Load();
                    sp.Play();
                }
                catch { }
            })).Start();
        }

        #endregion
    }

}