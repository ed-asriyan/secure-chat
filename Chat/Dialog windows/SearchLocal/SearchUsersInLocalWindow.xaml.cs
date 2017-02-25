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
    /// Interaction logic for SearchUsersInLocalWindow.xaml
    /// </summary>
    public partial class SearchUsersInLocalWindow : Window
    {
        #region Constructors

        public SearchUsersInLocalWindow()
        {
            InitializeComponent();

            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                try
                {
                    var ips = LocalComputersBase.Global.Update();
                    if (ips == null) throw new Exception();
                    this.Dispatcher.Invoke(() =>
                    {
                        this.lableSearch.Visibility = System.Windows.Visibility.Collapsed;
                        foreach (var ip in ips)
                        {

                            var t = new SearchLoacalItem(ip);
                            t.OnChatOpen += delegate()
                            {
                                this.Close();
                            };
                            this.Items.Children.Add(t);

                        }
                    });
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.lableSearch.Content = "Ошибка поиска";

                    });
                }
            })).Start();
        }

        #endregion
    }
}
