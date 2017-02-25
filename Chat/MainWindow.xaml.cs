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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Threading;

using System.Windows.Media.Animation;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Internal

        public const string PROGRAM_NAME = "Мессенджер";

        IPAddress _publicIp = null;
        IPAddress _localIp = null;

        Thread _updatingLocalList = null;
        bool _updatingLocalListAlive = true;

        System.Windows.Forms.NotifyIcon _notifyIcon = new System.Windows.Forms.NotifyIcon();

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            this.RestoreSession();

            this.PublicIP.ToolTip = "Внешний IP ещё не определился. Порты ещё не проброшены...";

            try
            {
                API.MainServer.StartServer();
            }
            catch
            {
                MessageBox.Show("Произошла ошибка во время открытия порта. Это может быть вызвано из-за того, что другая программа на компьютере уже использует этот порт. Поменяйте порт в настройках на другой и перезапустите программу.", "Порт уже прослушивается", MessageBoxButton.OK, MessageBoxImage.Error);
                SettingsButton_Click(null, null);
                mWindow_Closing(null, null);
            }

            NAT.Discover(() =>
            {
                bool ttt = false;
                this.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        this._publicIp = NAT.GetExternalIP();
                        if (this._publicIp != null)
                        {
                            this.PublicIP.Content = "Внешний IP: " + this._publicIp.ToString();
                            this.PublicIP.ToolTip = "Нажмите правой кнопкой мыши, чтобы скопировать в буфер обмена";
                            ttt = true;
                        }


                    }
                    catch { }

                }));
                if (!ttt) return;
                NAT.ForwardPort(Settings.Global.ServerPort, Settings.Global.ServerPort);
                if (this.IsInitialized)
                {
                    if (!NAT.IsMapped(Settings.Global.ServerPort, Settings.Global.ServerPort))
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.MappingErrorIcon.Visibility = Visibility.Visible;
                        }));
                    }
                    else
                    {
                        this.Dispatcher.Invoke(new Action(() =>
                        {
                            this.MappingErrorIcon.Visibility = Visibility.Collapsed;
                        }));

                        TransformScalIPLabels(0.8);
                        publicIPDetected = true;
                    }
                }
            });

            this._updatingLocalList = new Thread(new ThreadStart(() =>
            {
                while (this._updatingLocalListAlive)
                {
                    LocalComputersBase.Global.Update();
                    Thread.Sleep((int)Settings.Global.LocalComputersUpdateInterval);
                }
            }));
            this._updatingLocalList.Start();

            this._localIp = NAT.GetInternalIP();

            this.LocalIP.Content = "Локальный IP: " + this._localIp.ToString();
            this.LocalIP.ToolTip = "Нажмите правой кнопкой мыши, чтобы скопировать в буфер обмена";

            this.PublicIP.Content = "Внешний IP: " + " определяется...";


            this.UserName.Text = Environment.UserName;

            TransformScalIPLabels(1 / 0.8, 100);

            this.UserName.TextChanged += UserName_TextChanged;

            this.UserName.Text = Settings.Global.UserName;

            this._notifyIcon.Text = PROGRAM_NAME;
            this._notifyIcon.Icon = new System.Drawing.Icon(Properties.Resources.Icon, new System.Drawing.Size(50, 50));
            this._notifyIcon.Visible = Settings.Global.ShowTrayIcon;
            this._notifyIcon.Click += (object sender, EventArgs e) =>
            {
                this.WindowState = System.Windows.WindowState.Normal;
                this.ShowInTaskbar = true;
            };

            var arguments = Environment.CommandLine.Split(' ');
            foreach (var argument in arguments)
            {
                switch (argument.ToLower())
                {
                    case @"/minimized":

                        new Thread(new ThreadStart(() =>
                        {
                            Thread.Sleep(2000);
                            this.Dispatcher.Invoke(() =>
                            {
                                this.WindowState = System.Windows.WindowState.Minimized;
                            });
                        })).Start();
                        break;
                }
            }

            this.ProfilePhoto.Source = Settings.Global.UserPhoto;

        }

        #endregion

        #region IP Labels

        bool publicIPDetected = false;

        void TransformScalIPLabels(double scaleC, double Duration = 650)
        {
            if (this.IsInitialized)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {

                    TimeSpan duration = TimeSpan.FromMilliseconds(Duration);
                    double toValueL = scaleC;
                    double toValueP = 1 / toValueL;

                    ScaleTransform transLocal = new ScaleTransform();
                    ScaleTransform transPublic = new ScaleTransform();
                    LocalIP.RenderTransform = transLocal;
                    PublicIP.RenderTransform = transPublic;
                    
                    DoubleAnimation animLocal = new DoubleAnimation(toValueL, duration);
                    DoubleAnimation animPublic = new DoubleAnimation(toValueP, duration);

                    animLocal.DecelerationRatio = 0.8;
                    animPublic.DecelerationRatio = 0.8;

                    transLocal.BeginAnimation(ScaleTransform.ScaleXProperty, animLocal);
                    transLocal.BeginAnimation(ScaleTransform.ScaleYProperty, animLocal);
                    transPublic.BeginAnimation(ScaleTransform.ScaleYProperty, animPublic);
                    transPublic.BeginAnimation(ScaleTransform.ScaleXProperty, animPublic);

                }));
            }
        }

        private void LocalIP_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!publicIPDetected) return;
            TransformScalIPLabels(1 / 0.8, 300);
        }

        private void LocalIP_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!publicIPDetected) return;
            TransformScalIPLabels(0.8, 300);
        }

        #endregion

        #region Title

        public void UpdateTitle(ChatConnection chatConnection)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (chatConnection == null)
                {
                    this.Title = PROGRAM_NAME;
                }
                else
                {
                    this.Title = chatConnection.Name + " (" + chatConnection.IP.ToString() + ")   —   " + PROGRAM_NAME;
                }

            });
        }

        #endregion

        #region Routed events

        private void AddNewConnection_Click(object sender, RoutedEventArgs e)
        {
            AddConnectionWindow addWindow = new AddConnectionWindow();
            addWindow.Topmost = true;
            bool? result = addWindow.ShowDialog();
            if (result != null && result.Value)
            {
                var c = addWindow.ChatConnection;

                this.ChatArea.AddChatBox(c, true);
            }
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.Init(Settings.Global);

            Color backgroundChatBox = Settings.Global.ChatBoxBackgroundColor;
            Color backgroundProgram = Settings.Global.ProgramBackgroundColor;
            Color backgroundMessage = Settings.Global.MessageBackgroundColor;
            UInt16 port = Settings.Global.ServerPort;

            bool? result = sw.ShowDialog();

            if (result != null && result.Value)
            {
                Settings.Global = sw.SettingsValue;
                TimeSpan duration = TimeSpan.FromMilliseconds(230);

                if (Settings.Global.ChatBoxBackgroundColor != backgroundChatBox)
                {
                    ColorAnimation animBackgroundColor = new ColorAnimation(sw.SettingsValue.ChatBoxBackgroundColor, duration);
                    foreach (var chatBox in this.ChatArea.ChatBoxList)
                    {
                        chatBox.MessagesAreaViewer.Background.BeginAnimation(SolidColorBrush.ColorProperty, animBackgroundColor);// = //new SolidColorBrush(sw.SettingsValue.ChatBoxBackgroundColor);
                    }
                }

                if (Settings.Global.ProgramBackgroundColor != backgroundProgram)
                {
                    ColorAnimation animBackgroundColor = new ColorAnimation(sw.SettingsValue.ProgramBackgroundColor, duration);

                    this.Background.BeginAnimation(SolidColorBrush.ColorProperty, animBackgroundColor);
                }

                if (Settings.Global.MessageBackgroundColor != backgroundMessage)
                {
                    ColorAnimation animBackgroundColor = new ColorAnimation(sw.SettingsValue.MessageBackgroundColor, duration);
                    foreach (var chatBox in this.ChatArea.ChatBoxList)
                    {
                        foreach (var mc in chatBox.MessagesArea.Children)
                        {
                            if (mc is MessageControl)
                            {
                                (mc as MessageControl).BubbleBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, animBackgroundColor);// = //new SolidColorBrush(sw.SettingsValue.ChatBoxBackgroundColor);
                            }
                        }
                        foreach (var mc in chatBox.SendingMessages.Children)
                        {
                            if (mc is MessageControl)
                            {
                                (mc as MessageControl).BubbleBorder.Background.BeginAnimation(SolidColorBrush.ColorProperty, animBackgroundColor);// = //new SolidColorBrush(sw.SettingsValue.ChatBoxBackgroundColor);
                            }
                        }
                    }
                }

                if (Settings.Global.ServerPort != port)
                {
                    MessageBox.Show("Для того, чтобы новые настройки были примены полностью, перезапустите программу.");
                }

                Settings.Global = sw.SettingsValue;
                this._notifyIcon.Visible = Settings.Global.ShowTrayIcon;
                this.UserName.Text = Settings.Global.UserName;
                this.ProfilePhoto.Source = Settings.Global.UserPhoto;
            }
        }

        private void SearchLocalComputers_Click(object sender, RoutedEventArgs e)
        {
            var sw = new SearchUsersInLocalWindow();

            sw.ShowDialog();
        }

        private void LogOpenButton_Click(object sender, RoutedEventArgs e)
        {
            var sw = new HistoryViewer();
            sw.Update();

            sw.ShowDialog();
        }

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            API.MainServer.UserName = (sender as EditedLabel).Text;
        }



        private void LocalIP_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this._localIp != null)
            {
                Clipboard.SetText(this._localIp.ToString());
            }
        }

        private void PublicIP_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this._publicIp != null)
            {
                Clipboard.SetText(this._publicIp.ToString());
            }
        }

        private void mWindow_StateChanged(object sender, EventArgs e)
        {
            if (Settings.Global.ShowTrayIcon)
            {
                if (this.WindowState == System.Windows.WindowState.Minimized)
                {
                    this.ShowInTaskbar = false;
                }
                else
                {
                    this.ShowInTaskbar = true;
                }
            }
        }

        private void ProfilePhoto_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog cd = new System.Windows.Forms.OpenFileDialog();
            cd.Title = "Выбор фотографии";
            cd.Filter = "Изображения (*.png, *.jpg, *.jpeg, *.bmp)|*.bmp;*.jpg;*.jpeg;*png";

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!System.IO.File.Exists(cd.FileName))
                {
                    MessageBox.Show("Файл не найден.");
                    return;
                }

                FileInfo fi = new FileInfo(cd.FileName);
                if (fi.Length > 4124672)
                {
                    MessageBox.Show("Размер изображения не должен привышать 2 МБ.");
                    return;
                }

                Settings.Global.UserPhotoBytes = System.IO.File.ReadAllBytes(cd.FileName);
                this.ProfilePhoto.Source = new BitmapImage(new Uri(cd.FileName));
            }
        }

        private void mWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (e != null && Settings.Global.AskForExit && MessageBox.Show("Вы действительно хотите закрыть " + PROGRAM_NAME + "?", "Закрытие", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            NetServer.MainServer.Longpoll.Client.RemoveAll();

            this.WindowState = System.Windows.WindowState.Minimized;

            this.SaveSession();

            API.MainServer.StopServer();

            NAT.DeleteForwardingRule(Settings.Global.ServerPort, Settings.Global.ServerPort);

            this._notifyIcon.Visible = false;

            Application.Current.Shutdown();
            System.Diagnostics.Process.GetCurrentProcess().Kill(); // not good
        }

        #endregion

        #region Session


        /// <summary>
        /// Сохраняет текущую сессию на диск
        /// </summary>
        public void SaveSession()
        {
            try
            {
                List<Trio<ChatConnection, Message[], StrokeLikeList>> chats = new List<Trio<ChatConnection, Message[], StrokeLikeList>>();

                if (Settings.Global.SaveConnections)
                {
                    foreach (var chat in this.ChatArea.ChatBoxList)
                    {
                        chats.Add(new Trio<ChatConnection, Message[], StrokeLikeList>(chat.ChatConnection, Settings.Global.SaveMessages ? chat.Messages : null, Settings.Global.SaveMessages ? new StrokeLikeList(chat.addbrInkCanvas.Strokes) : null));
                        chat.Terminate();
                    }

                }

                FileStream fs = new FileStream(Environment.CurrentDirectory + @"\" + "Session.bin", FileMode.Create);

                Quadruplet<ConnectionBase, PasswordsBase, AttachmentsBase, LongpollOnlyConnectionBase> bases = Settings.Global.SaveMessages ? new Quadruplet<ConnectionBase, PasswordsBase, AttachmentsBase, LongpollOnlyConnectionBase>(ConnectionBase.Main, PasswordsBase.Main, AttachmentsBase.Main, LongpollOnlyConnectionBase.Global) : null;

                new BinaryFormatter().Serialize(fs, new Trio<Settings, Quadruplet<ConnectionBase, PasswordsBase, AttachmentsBase, LongpollOnlyConnectionBase>, List<Trio<ChatConnection, Message[], StrokeLikeList>>>(Settings.Global, bases, chats));

                fs.Close();
            }
            catch { }
        }

        /// <summary>
        /// Восстанавливает предыдущую сессию с диска
        /// </summary>
        public void RestoreSession()
        {
            try
            {
                Trio<Settings, Quadruplet<ConnectionBase, PasswordsBase, AttachmentsBase, LongpollOnlyConnectionBase>, List<Trio<ChatConnection, Message[], StrokeLikeList>>> t = null;
                if (System.IO.File.Exists(Environment.CurrentDirectory + @"\" + "Session.bin"))
                {
                    try
                    {
                        FileStream fs = new FileStream(Environment.CurrentDirectory + @"\" + "Session.bin", FileMode.Open);

                        t = new BinaryFormatter().Deserialize(fs) as Trio<Settings, Quadruplet<ConnectionBase, PasswordsBase, AttachmentsBase, LongpollOnlyConnectionBase>, List<Trio<ChatConnection, Message[], StrokeLikeList>>>;

                        fs.Close();
                    }
                    catch { }
                }
                else
                {
                    this.GetStartedImage.Visibility = System.Windows.Visibility.Visible;
                }

                if (t != null)
                {
                    if (t.t1 != null)
                    {
                        Settings.Global = t.t1;
                    }

                    if (t.t2.t2 != null)
                    {
                        PasswordsBase.Main = t.t2.t2;
                    }

                    if (t.t2.t1 != null)
                    {
                        ConnectionBase.Main = t.t2.t1;
                    }

                    if (t.t2.t3 != null)
                    {
                        AttachmentsBase.Main = t.t2.t3;
                    }

                    if (t.t2.t4 != null)
                    {
                        LongpollOnlyConnectionBase.Global = t.t2.t4;
                    }

                }
                App.MainWindow = this;
                App.MainControlTabs = this.ChatArea;
                App.NotifyIcon = this._notifyIcon;

                NetServer.MainServer = new NetServer(Settings.Global.ServerPort);
                API.MainServer = new API();

                this.ChatArea.Init();

                bool sound = Settings.Global.PlayMessageSound;
                Settings.Global.PlayMessageSound = false;

                if (t != null && t.t3 != null)
                {
                    foreach (var chat in t.t3)
                    {
                        chat.t1.IsBindedWithBases = false;
                        var chatBox = this.ChatArea.AddChatBox(chat.t1, false);
                        if (chat.t2 != null)
                        {
                            foreach (var m in chat.t2)
                            {
                                chatBox.AddMessage(m);
                            }
                        }
                        if (chat.t3 != null)
                        {
                            chatBox.SetCanvasStrokes(chat.t3.ConventToStrokeCollection(), false);
                        }
                    }
                }

                Settings.Global.PlayMessageSound = sound;
                this.Background = new SolidColorBrush(Settings.Global.ProgramBackgroundColor);
            }
            catch { }
        }

        #endregion
    }
}
