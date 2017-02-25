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
using System.Net;
using System.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for SearchLoacalItem.xaml
    /// </summary>
    public partial class SearchLoacalItem : UserControl
    {
        #region Events

        public delegate void OnOpenChat();
        public event OnOpenChat OnChatOpen;

        #endregion

        #region Public values

        IPAddress ip { get; set; }

        #endregion

        #region Constructors

        public SearchLoacalItem(IPAddress ipAddress)
        {
            InitializeComponent();

            this.IP.Text = ipAddress.ToString();

            this.ip = ipAddress;
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    string name = API.MainServer.GetClientName(ipAddress, Settings.ServerPortDefault);

                    this.Dispatcher.Invoke(() =>
                    {
                        this.NameUser.Text = name;
                        this.LoadingIcon.Visibility = System.Windows.Visibility.Collapsed;
                        this.NameUser.Visibility = System.Windows.Visibility.Visible;
                        this.OpenChat.Visibility = System.Windows.Visibility.Visible;
                    });
                }
                catch (Exception e)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.ErrorIcon.ToolTip = e.Message;
                        this.LoadingIcon.Visibility = System.Windows.Visibility.Collapsed;
                        this.ErrorIcon.Visibility = System.Windows.Visibility.Visible;
                        this.Visibility = System.Windows.Visibility.Collapsed;
                    });
                }
            })).Start();
        }

        #endregion

        #region Routed events

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            App.MainControlTabs.AddChatBox(this.ip, Settings.ServerPortDefault, true);
            if (this.OnChatOpen != null)
            {
                this.OnChatOpen();
            }
        }

        #endregion
    }
}
