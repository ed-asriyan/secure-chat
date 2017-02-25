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
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Animation;

namespace Chat
{
    /// <summary>
    /// Interaction logic for NewMessageNotificationWindow.xaml
    /// </summary>
    public partial class NewMessageNotificationWindow : Window
    {
        #region Private values

        Message message = null;

        #endregion

        #region Constructors

        public NewMessageNotificationWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void Init(Message message)
        {
            if (message == null) throw new ArgumentNullException("message");

            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right + 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            this.message = message;

            this.Autor.Text = message.AutorName;
            this.MessageBody.Text = message.Body;
            this.ProfilePhoto.Source = message.chatConnetcion.ProfilePhoto;

            if (message.Attachment != null)
            {
                if (!string.IsNullOrWhiteSpace(message.Body)) this.MessageBody.Text += Environment.NewLine;
                this.MessageBody.Text += "[ Вложение ]";
            }

            Utility.UI.BeginVisibilityAnimation(this, true, 150);

            DoubleAnimation moving = new DoubleAnimation();
            moving.Duration = TimeSpan.FromMilliseconds(135);
            moving.From = this.Left;
            moving.To = desktopWorkingArea.Right - this.Width - 10;

            this.BeginAnimation(LeftProperty, moving);

            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(Settings.Global.NotificationShowingTimeSpan);
                this.CloseFade();
            })).Start();


        }

        #endregion

        #region Events

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            App.MainWindow.WindowState = System.Windows.WindowState.Normal;
            App.MainControlTabs.SetOnTop(App.MainControlTabs.Find(message.chatConnetcion));


        }
        void CloseFade()
        {
            this.Dispatcher.Invoke(() =>
            {
                Utility.UI.BeginVisibilityAnimation(this, false, 200);
                new Thread(new ThreadStart(() =>
                {
                    Thread.Sleep(200);
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                })).Start();
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseFade();
        }

        #endregion
    }
}
