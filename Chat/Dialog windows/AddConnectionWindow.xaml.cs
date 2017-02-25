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

using System.Net;

namespace Chat
{
    /// <summary>
    /// Interaction logic for AddConnectionWindow.xaml
    /// </summary>
    public partial class AddConnectionWindow : Window
    {
        #region Constructors

        public AddConnectionWindow()
        {
            InitializeComponent();

            this.ConnectionIP.Focus();
            this.ConnectionPort.Text = Settings.Global.ServerPort.ToString();
        }

        #endregion

        #region Public values

        public ChatConnection ChatConnection { get; private set; }

        #endregion

        #region Routed events

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ChatConnection = new ChatConnection(IPAddress.Parse(this.ConnectionIP.Text), UInt16.Parse(this.ConnectionPort.Text), false);
                this.ChatConnection.LongPoll = this.LongPoll.IsChecked.Value;
                this.ChatConnection.SetSecretKey(this.ConnectionKey.Text);
                this.ChatConnection.EncruptionIsNeeded = this.Encryption.IsChecked.Value;
                this.ChatConnection.UseLongpollOnly = this.UseLOngpollOnly.IsChecked.Value;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Введён некорректный IP адрес или порт.", "Данные введены некорректно", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

        private void Cansel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Режим без шифрования не является безопасным." + Environment.NewLine +
                "Для того, чтобы связаться с собеседником без использования шифрования необходимо, чтобы он добавил Вас в список контактов и отключил шифрование в параметрах соединения с Вами." + (!this.Encryption.IsEnabled ? (Environment.NewLine + Environment.NewLine + "Функция отключена, т.к. в параметрах отключено шифрование с устройствами в локальной сети.") : ""), "Незащищённое соединение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private void Encryption_Checked(object sender, RoutedEventArgs e)
        {
            this.EncryptionAttention.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Encryption_Unchecked(object sender, RoutedEventArgs e)
        {
            this.EncryptionAttention.Visibility = System.Windows.Visibility.Visible;
        }

        bool _encryptionCheched = true;
        private void ConnectionIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Settings.Global.DisableEncryptionWithLocalComputers && LocalComputersBase.Global.Contains(this.ConnectionIP.Text))
            {
                if (this.Encryption.IsChecked.Value) this._encryptionCheched = true;
                this.Encryption.IsChecked = false;
                this.Encryption.IsEnabled = false;
                this.ConnectionKey.Text = string.Empty;
            }
            else
            {
                this.Encryption.IsEnabled = true;                
            }
        }

        #endregion
    }
}