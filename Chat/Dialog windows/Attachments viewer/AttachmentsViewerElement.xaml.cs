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

namespace Chat
{
    /// <summary>
    /// Interaction logic for AttachmentsViewerElement.xaml
    /// </summary>
    public partial class AttachmentsViewerElement : UserControl
    {
        #region Events

        public delegate void OnDeleteAttachmentDelegate();
        public event OnDeleteAttachmentDelegate OnDeleteAttachment;

        #endregion

        #region Public values

        public Attachment Attachment { get; set; }
        public string Ip
        {
            get
            {
                return this.IP.Text;
            }
        }

        #endregion

        #region Constructors

        public AttachmentsViewerElement()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        public void Init(Attachment attachment)
        {
            this.Attachment = attachment;

            this.IP.Text = attachment.IP.ToString();
            this.FileName.Text = attachment.Name;
            this.FileName.ToolTip = attachment.Path;
            this.DownloadsCount.Text = attachment.DownloadCount.ToString();
            this.FileExt.Text = attachment.extession;

            this.Unregister.Click += (object sender, RoutedEventArgs e) =>
            {
                AttachmentsBase.Main.UnregisterAttachment(attachment.IP, attachment.Id);
                if (this.OnDeleteAttachment != null)
                {
                    this.OnDeleteAttachment();
                }
            };

            this.OpenFolder.Click += (object sender, RoutedEventArgs e) =>
            {
                System.Diagnostics.Process.Start("explorer", "/select," + this.Attachment.Path);
            };
        }

        #endregion
    }
}
