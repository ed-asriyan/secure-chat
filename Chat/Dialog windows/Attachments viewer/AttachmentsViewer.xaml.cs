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
    /// Interaction logic for AttachmentsViewer.xaml
    /// </summary>
    public partial class AttachmentsViewer : Window
    {
        #region Constructors

        public AttachmentsViewer()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        public void Init(IPAddress ipAddress = null)
        {
            Init(ipAddress == null ? null : ipAddress.ToString());
        }

        public void Init(string ipAddress = null)
        {
            var d = AttachmentsBase.Main.GetAllAttachments();

            foreach (var a in d)
            {
                AttachmentsViewerElement ave = new AttachmentsViewerElement();
                ave.Init(a);

                ave.OnDeleteAttachment += () =>
                {
                    this.StackPanel.Children.Remove(ave);
                };

                this.StackPanel.Children.Add(ave);
            }

            if (!string.IsNullOrWhiteSpace(ipAddress)) this.IpSearch.Text = ipAddress;
        }

        #endregion

        #region Routed events

        private void IpSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            for (int i = 0; i < this.StackPanel.Children.Count; i++)
            {
                if (string.IsNullOrEmpty(this.IpSearch.Text) || (this.StackPanel.Children[i] as AttachmentsViewerElement).Ip.ToLower().Contains(this.IpSearch.Text.ToLower()))
                {
                    (this.StackPanel.Children[i] as AttachmentsViewerElement).Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    (this.StackPanel.Children[i] as AttachmentsViewerElement).Visibility = System.Windows.Visibility.Collapsed;
                }
            }
        }

        #endregion
    }
}
