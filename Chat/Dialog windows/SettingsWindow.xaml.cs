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
using System.Windows.Media.Animation;

namespace Chat
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region Public values

        public Settings SettingsValue { get; private set; }

        #endregion

        #region Constructors

        public SettingsWindow()
        {
        }

        #endregion

        #region Initialization

        public void Init(Settings Settings)
        {
            InitializeComponent();

            this.SettingsValue = Settings;

            this.UserName.Text = SettingsValue.UserName;
            this.ServerPort.Text = SettingsValue.ServerPort.ToString();
            this.AlowOtherMessages.IsChecked = SettingsValue.recieveMessagesFromOthers;
            this.AlowOtherConnections.IsChecked = SettingsValue.recieveConnectionsFromOthers;
            this.SecretWord.Text = string.IsNullOrWhiteSpace(SettingsValue.DefaultSecretKey) ? string.Empty : Settings.Global.DefaultSecretKey;
            this.IntervalOnlineCheck.Text = SettingsValue.OnlineCheckInterval.ToString();
            this.MaxFileSize.Text = SettingsValue.MaxFilesSize.ToString();
            this.MaxPhotoSize.Text = SettingsValue.MaxPhotosSize.ToString();
            this.LongpollThreadsCount.Text = SettingsValue.LongpollSimultaneousThreads.ToString();
            this.LongpollTimeout.Text = SettingsValue.LongpollResponseTimeout.ToString();
            this.MessageSound.IsChecked = SettingsValue.PlayMessageSound;
            this.EncryptionBlock.Text = SettingsValue.EncryptionBlock.ToString();
            this.ConnectionTimeout.Text = SettingsValue.RECIEVE_TIMEOUT.ToString();
            this.UpperCase.IsChecked = SettingsValue.UpperCaseForDefault;
            this.LocalComputersUpdateInterval.Text = SettingsValue.LocalComputersUpdateInterval.ToString();
            this.DisableEncryptionWhithLocalComputers.IsChecked = SettingsValue.DisableEncryptionWithLocalComputers;
            this.ShowTrayIcon.IsChecked = SettingsValue.ShowTrayIcon;
            this.ShowMessagesNotifications.IsChecked = SettingsValue.ShowPopupNotifications;
            this.AskForExit.IsChecked = SettingsValue.AskForExit;
            this.ProfilePhoto.Source = SettingsValue.UserPhoto;
            this.NotificationTimeSpan.Text = SettingsValue.NotificationShowingTimeSpan.ToString();

            this.MessageBackground.Fill = new SolidColorBrush(SettingsValue.MessageBackgroundColor);
            this.ProgramBackground.Fill = new SolidColorBrush(SettingsValue.ProgramBackgroundColor);
            this.ChatBackground.Fill = new SolidColorBrush(SettingsValue.ChatBoxBackgroundColor);
        }

        #endregion

        #region OK button Click

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            if (this.PortToDefaultErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.UserNameErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.MaxFileSizeErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.MaxPhotoSizeErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.LongpollThreadsCountErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.LongpollTimeoutErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.EncryptionBlockErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.ConnectionTimeoutErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.IntervalOnlineCheckErrorIcon.Visibility == System.Windows.Visibility.Visible ||
                this.LocalComputersUpdateIntervalErrorIcon.Visibility == System.Windows.Visibility.Visible
                )
            {
                MessageBox.Show("Некоторые поля введены некорректно", "Некорректно введены данные", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                SettingsValue.UserName = this.UserName.Text;
                SettingsValue.ServerPort = UInt16.Parse(this.ServerPort.Text);
                SettingsValue.recieveMessagesFromOthers = this.AlowOtherMessages.IsChecked.Value;
                SettingsValue.recieveConnectionsFromOthers = this.AlowOtherConnections.IsChecked.Value;
                SettingsValue.DefaultSecretKey = string.IsNullOrWhiteSpace(this.SecretWord.Text) ? null : this.SecretWord.Text;
                SettingsValue.MaxPhotosSize = uint.Parse(this.MaxPhotoSize.Text);
                SettingsValue.MaxFilesSize = uint.Parse(this.MaxFileSize.Text);
                SettingsValue.LongpollResponseTimeout = int.Parse(this.LongpollTimeout.Text);
                SettingsValue.LongpollSimultaneousThreads = int.Parse(this.LongpollThreadsCount.Text);
                SettingsValue.PlayMessageSound = this.MessageSound.IsChecked.Value;
                SettingsValue.RECIEVE_TIMEOUT = int.Parse(this.ConnectionTimeout.Text);
                SettingsValue.SEND_TIMEOUT = int.Parse(this.ConnectionTimeout.Text);
                SettingsValue.EncryptionBlock = int.Parse(this.EncryptionBlock.Text);
                SettingsValue.UpperCaseForDefault = this.UpperCase.IsChecked.Value;
                SettingsValue.OnlineCheckInterval = uint.Parse(this.IntervalOnlineCheck.Text);
                SettingsValue.DisableEncryptionWithLocalComputers = this.DisableEncryptionWhithLocalComputers.IsChecked.Value;
                SettingsValue.OnlineCheckInterval = uint.Parse(this.IntervalOnlineCheck.Text);
                SettingsValue.ShowTrayIcon = this.ShowTrayIcon.IsChecked.Value;
                SettingsValue.ShowPopupNotifications = this.ShowMessagesNotifications.IsChecked.Value;
                SettingsValue.AskForExit = this.AskForExit.IsChecked.Value;
                SettingsValue.NotificationShowingTimeSpan = int.Parse(this.NotificationTimeSpan.Text);

                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        #endregion

        #region Color boxes

        private void ChatBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AnyColor = true;
            cd.AllowFullOpen = true;
            cd.Color = System.Drawing.Color.FromArgb(Settings.Global.ChatBoxBackgroundColor.A, Settings.Global.ChatBoxBackgroundColor.R, Settings.Global.ChatBoxBackgroundColor.G, Settings.Global.ChatBoxBackgroundColor.B);

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = cd.Color;

                this.ChatBackground.Fill = new SolidColorBrush(Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));

                SettingsValue.ChatBoxBackgroundColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
            }
        }

        private void ProgramBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            var _tColor = Settings.Global.ProgramBackgroundColor;
            cd.Color = System.Drawing.Color.FromArgb(_tColor.A, _tColor.R, _tColor.G, _tColor.B);

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = cd.Color;

                this.ProgramBackground.Fill = new SolidColorBrush(Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));
                SettingsValue.ProgramBackgroundColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
            }
        }

        private void MessageBackground_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            var _tColor = Settings.Global.MessageBackgroundColor;
            cd.Color = System.Drawing.Color.FromArgb(_tColor.A, _tColor.R, _tColor.G, _tColor.B);

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = cd.Color;

                this.MessageBackground.Fill = new SolidColorBrush(Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B));
                SettingsValue.MessageBackgroundColor = Color.FromArgb(cd.Color.A, cd.Color.R, cd.Color.G, cd.Color.B);
            }
        }

        private void Cansel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        #endregion

        #region Routed events

        uint animationDuration = 150;

        private void UserName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.UserName.Text) && this.UserNameErrorIcon.Visibility != System.Windows.Visibility.Visible)
            {
                Utility.UI.BeginVisibilityAnimation(this.UserNameErrorIcon, true, animationDuration);
            }
            else if (!string.IsNullOrWhiteSpace(this.UserName.Text) && this.UserNameErrorIcon.Visibility == System.Windows.Visibility.Visible)
            {
                Utility.UI.BeginVisibilityAnimation(this.UserNameErrorIcon, false, animationDuration);
            }
        }

        private void ServerPort_TextChanged(object sender, TextChangedEventArgs e)
        {
            UInt16 t = 0;
            if (!UInt16.TryParse(this.ServerPort.Text, out t) || t == 0)
            {
                if (this.PortToDefaultErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.PortToDefaultErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.PortToDefaultErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.PortToDefaultErrorIcon, false, animationDuration);
                }
            }
        }

        private void MinPhotoSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = -1;
            if (!int.TryParse(this.MaxPhotoSize.Text, out t) || t < 0)
            {
                if (this.MaxPhotoSizeErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.MaxPhotoSizeErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.MaxPhotoSizeErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.MaxPhotoSizeErrorIcon, false, animationDuration);
                }
            }
        }

        private void MaxFileSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = -1;
            if (!int.TryParse(this.MaxFileSize.Text, out t) || t < 0)
            {
                if (this.MaxFileSizeErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.MaxFileSizeErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.MaxFileSizeErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.MaxFileSizeErrorIcon, false, animationDuration);
                }
            }
        }

        private void LongpollThreadsCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.LongpollThreadsCount.Text, out t) || t <= 0)
            {
                if (this.LongpollThreadsCountErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LongpollThreadsCountErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.LongpollThreadsCountErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LongpollThreadsCountErrorIcon, false, animationDuration);
                }
            }
        }

        private void LongpollTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.LongpollTimeout.Text, out t) || t <= 0)
            {
                if (this.LongpollTimeoutErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LongpollTimeoutErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.LongpollTimeoutErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LongpollTimeoutErrorIcon, false, animationDuration);
                }
            }
        }

        private void ConnectionTimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.ConnectionTimeout.Text, out t) || t <= 0)
            {
                if (this.ConnectionTimeoutErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.ConnectionTimeoutErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.ConnectionTimeoutErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.ConnectionTimeoutErrorIcon, false, animationDuration);
                }
            }
        }

        private void EncryptionBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.EncryptionBlock.Text, out t) || t <= 0)
            {
                if (this.EncryptionBlockErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.EncryptionBlockErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.EncryptionBlockErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.EncryptionBlockErrorIcon, false, animationDuration);
                }
            }

        }

        private void IntervalOnlineCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.IntervalOnlineCheck.Text, out t) || t <= 0)
            {
                if (this.IntervalOnlineCheckErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.IntervalOnlineCheckErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.IntervalOnlineCheckErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.IntervalOnlineCheckErrorIcon, false, animationDuration);
                }
            }
        }

        private void LocalComputersUpdateInterval_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.LocalComputersUpdateInterval.Text, out t) || t <= 0)
            {
                if (this.LocalComputersUpdateIntervalErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LocalComputersUpdateIntervalErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.LocalComputersUpdateIntervalErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.LocalComputersUpdateIntervalErrorIcon, false, animationDuration);
                }
            }
        }

        private void NotificationTimeSpan_TextChanged(object sender, TextChangedEventArgs e)
        {
            int t = 0;
            if (!int.TryParse(this.NotificationTimeSpan.Text, out t) || t <= 0)
            {
                if (this.NotificationTimeSpanErrorIcon.Visibility != System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.NotificationTimeSpanErrorIcon, true, animationDuration);
                }
            }
            else
            {
                if (this.NotificationTimeSpanErrorIcon.Visibility == System.Windows.Visibility.Visible)
                {
                    Utility.UI.BeginVisibilityAnimation(this.NotificationTimeSpanErrorIcon, false, animationDuration);
                }
            }
        }

        private void PortToDefault_Click(object sender, RoutedEventArgs e)
        {
            this.ServerPort.Text = Settings.ServerPortDefault.ToString();
        }


        private void AlowOtherConnectionsHelp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Если галочка снята, то к вашему компьютеру смогут подключиться только те, кто есть в вашем списке контактов (диалогов).", "Инфо", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private void AlowOtherMessagesHelp_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Если галочка снята, то к вашему компьютеру подключиться, смогут все, однако Вы увидите только те сообщения, которые пришли от людей, которые есть в вашем списке контактов (диалогов).", "Инфо", MessageBoxButton.OK, MessageBoxImage.Asterisk);
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

                var fi = new System.IO.FileInfo(cd.FileName);
                if (fi.Length > 4124672)
                {
                    MessageBox.Show("Размер изображения не должен привышать 2 МБ.", "Фотография не подходит", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SettingsValue.UserPhotoBytes = System.IO.File.ReadAllBytes(cd.FileName);
                this.ProfilePhoto.Source = new BitmapImage(new Uri(cd.FileName));
            }
        }

        private void ClearPhoto_Click(object sender, RoutedEventArgs e)
        {
            this.SettingsValue.UserPhotoBytes = null;

            this.ProfilePhoto.Source = SettingsValue.UserPhoto;
        }

        private void ClearParameters_Click(object sender, RoutedEventArgs e)
        {
            this.Init(new Settings());
        }

        #endregion
    }
}