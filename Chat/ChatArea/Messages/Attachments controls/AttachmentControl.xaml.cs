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
using System.Threading;
using System.IO;
using System.Windows.Media.Animation;

namespace Chat
{
    /// <summary>
    /// Interaction logic for AttachmentControl.xaml
    /// </summary>
    public partial class AttachmentControl : UserControl
    {
        #region Public static settings

        #region All files

    //    static bool AutoD

        #endregion

        #region Photos

        static string[] AutoDownloadingPhotosExtessions = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".JPG", ".JPEG", ".PNG", ".BMP" };

        #endregion

        #endregion

        #region Private values
 FrameworkElement clickableElement;

        string filePath;
        string fileDicrectory;
        bool isOut = true;

        #endregion

        #region Public values
        
        /// <summary>
        /// Прикреплённый чат
        /// </summary>
        public ChatConnection ChatConnection { get; set; }

        /// <summary>
        /// Прикреплённый файл
        /// </summary>
        public Attachment Attachment { get; set; }
        #endregion

        #region Constructors

        public AttachmentControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="chatConnection">Чат</param>
        /// <param name="attachment">Файл</param>
        /// <param name="isOut">Исходящий ли файл</param>
        public void Init(ChatConnection chatConnection, Attachment attachment, bool isOut)
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                this.ChatConnection = chatConnection;
                this.Attachment = attachment;
                this.isOut = isOut;

                this.FileName.Text = attachment.Name;
                this.txblExtession.Text = attachment.extession;
                this.txblFileSize.Text = Utility.Text.FileSizeToString(attachment.size, 4);

                if (!this.isOut)
                {
                    this.fileDicrectory = Environment.CurrentDirectory + @"\Загрузки\" + this.ChatConnection.IP.ToString();
                    this.filePath = this.fileDicrectory + @"\" + attachment.Name;
                }
                else
                {
                    this.fileDicrectory = System.IO.Path.GetDirectoryName(attachment.Path);
                    this.filePath = attachment.Path;
                }

                this.clickableElement = this.FileDownload;

                if (isOut)
                {
                    SetValuesAsDownloadedMode();
                }
                else
                {
                    if (this.PhotoDownloadingIsNeeded() || this.FileDownloadingIsNeeded())
                    {
                        var t = new System.IO.FileInfo(filePath);
                        if (System.IO.File.Exists(filePath) && t.Length == attachment.size)
                        {
                            SetValuesAsDownloadedMode();
                        }
                        else
                        {
                            this.DownloadFile();
                            SetValuesAsNoDownloadedMode();
                        }
                    }
                    else
                    {
                        SetValuesAsNoDownloadedMode();
                    }

                }
            }));
        }

        #endregion

        #region Attachment preview

        bool PhotoDownloadingIsNeeded()
        {
            return Settings.Global.MaxPhotosSize * 1024*1024 >= this.Attachment.size && AttachmentControl.AutoDownloadingPhotosExtessions.Contains(this.Attachment.extession);
        }

        bool FileDownloadingIsNeeded()
        {
            return Settings.Global.MaxFilesSize * 1024 * 1024 >= this.Attachment.size;
        }

        void ShowPreviewPhoto(string path)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    ScaleTransform st = new ScaleTransform();
                    st.ScaleY = 0;

                    this.PreviewImage.LayoutTransform = st;
                    this.PreviewImage.Opacity = 0;

                    FileStream fs= new FileStream(path, FileMode.Open, FileAccess.Read);
                    var img = BitmapFrame.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    if (img.PixelHeight < this.PreviewImage.MaxHeight) this.PreviewImage.Height = img.PixelHeight;
                    this.PreviewImage.Source = img;
                    fs.Close();

                    TimeSpan durationHeight = TimeSpan.FromMilliseconds(220);
                    TimeSpan durationOpacity = TimeSpan.FromMilliseconds(130);

                    DoubleAnimation animHeight = new DoubleAnimation(1, durationHeight);
                    DoubleAnimation animOpacity = new DoubleAnimation(1, durationOpacity);
                    animOpacity.BeginTime = TimeSpan.FromMilliseconds(75);


                    this.PreviewImage.Visibility = Visibility.Visible;
                    this.PreviewImage.BeginAnimation(OpacityProperty, animOpacity);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, animHeight);

                    this.clickableElement = this.PreviewImage;

                    this.InfoGrid.Visibility = Visibility.Collapsed;
                }
                catch { }
            }));
        }

        #endregion

        #region Helper methods

        void SetValuesAsDownloadedMode()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.PhotoDownloadingIsNeeded())
                {
                    ShowPreviewPhoto(this.filePath);
                }
                this.clickableElement.MouseLeftButtonDown -= OpenFile;
                this.clickableElement.MouseLeftButtonDown -= DownloadFile;

                this.clickableElement.MouseLeftButtonDown += OpenFile;

                this.FileDownload.Source = OpenImage();

                this.FileDownload.IsEnabled = true;
                this.FileDownload.ToolTip = "Открыть";
                this.grdDownloadingInfo.Visibility = Visibility.Collapsed;

                HideDownloadingPanel();
                Utility.UI.BeginWidthAnimation(this.FileShowInFolder, true, 200);
            });
        }

        void SetValuesAsNoDownloadedMode()
        {
            if (isOut) return;
            this.Dispatcher.Invoke(() =>
            {
                this.clickableElement.MouseLeftButtonDown -= OpenFile;
                this.clickableElement.MouseLeftButtonDown -= DownloadFile;

                this.clickableElement.MouseLeftButtonDown += DownloadFile;

                this.FileDownload.Source = DownloadImage();

                this.FileDownload.IsEnabled = true;
                this.FileDownload.ToolTip = "Скачать";
                {
                    this.HideDownloadingPanel();
                }

                Utility.UI.BeginWidthAnimation(this.FileShowInFolder, false, 200);
            });
        }

        void SetValuesAsDownloadingMode()
        {
            if (isOut) return;
            this.Dispatcher.Invoke(() =>
            {
                this.clickableElement.MouseLeftButtonDown -= OpenFile;
                this.clickableElement.MouseLeftButtonDown -= DownloadFile;

                this.clickableElement.MouseLeftButtonDown += StopDownloading;

                this.FileDownload.Source = CanselDownloading();

                this.FileDownload.IsEnabled = true;
                this.FileDownload.ToolTip = "Остановить";
                HideErrorPanel();
             //   if (this.grdDownloadingInfo.Visibility == System.Windows.Visibility.Collapsed)
                {
                    ShowDownloadingPanel();
                }

                Utility.UI.BeginWidthAnimation(this.FileShowInFolder, false, 200);
            });
        }

        void ShowError(string errorText)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.ErrorDescription.Text = errorText;

             //   if (this.grdDownloadingInfo.Visibility == System.Windows.Visibility.Visible)
                {
                    this.HideDownloadingPanel();
                }
                ShowErrorPanel();
            });
        }

        #endregion

        #region Downloading file

        void DownloadFile(object sender, EventArgs e)
        {
            DownloadFile();
        }

        Thread _gettingFileThread = null;

        void StopDownloading()
        {
            if (this._gettingFileThread != null)
            {
                if (this._gettingFileThread.IsAlive)
                {
                    this._gettingFileThread.Abort();

                    SetValuesAsNoDownloadedMode();
                }
            }
        }

        void StopDownloading(object sender, EventArgs e)
        {
            StopDownloading();
        }

        /// <summary>
        /// Скачивает файл
        /// </summary>
        /// <param name="sleep"></param>
        void DownloadFile()
        {
            StopDownloading();

            this._gettingFileThread = new Thread(new ThreadStart(() =>
            {
                bool a = false;

                SetValuesAsDownloadingMode();

                try
                {
                    if (!System.IO.Directory.Exists(fileDicrectory))
                    {
                        System.IO.Directory.CreateDirectory(fileDicrectory);
                    }

                    this.ChatConnection.DownloadAttachment(this.Attachment.Id, filePath, (object prSender, ProcessEventArgs pre) =>
                    {
                        try
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                this.prbrFileDownloadingProcess.Minimum = pre.Min;
                                this.prbrFileDownloadingProcess.Maximum = pre.Max;

                                TimeSpan duration = TimeSpan.FromMilliseconds(240);

                                DoubleAnimation animValue = new DoubleAnimation((double)pre.Value, duration);
                                animValue.DecelerationRatio = 0.8;

                                this.prbrFileDownloadingProcess.BeginAnimation(ProgressBar.ValueProperty, animValue);

                                this.txblFileDownloadingProgress.Text = Utility.Text.FileSizeToString(pre.Value - pre.Min, 3) + " / " + Utility.Text.FileSizeToString(pre.Max - pre.Min, 3);
                                this.txblFileDownloadingPercent.Text = ((int)(((double)(pre.Value - pre.Min) / (double)(pre.Max - pre.Min)) * 100)).ToString() + "%";
                            });
                        }
                        catch (Exception ex)
                        {
                            ShowError(ex.Message);
                            SetValuesAsNoDownloadedMode();
                        }
                    });

                    this.SetValuesAsDownloadedMode();
                }
                catch (Exception ex)
                {
                    ShowError(ex.Message);
                    SetValuesAsNoDownloadedMode();
                    return;
                }

            })); this._gettingFileThread.Start();
        }

        void OpenFile(object sender, EventArgs e)
        {
            try
            {
                if (PhotoDownloadingIsNeeded() && this.PreviewImage.Visibility != System.Windows.Visibility.Visible)
                {
                    SetValuesAsDownloadedMode();
                    return;
                }
                System.Diagnostics.Process.Start(filePath);
            }
            catch (Exception ex)
            {
                this.ShowError("Не удаётся найти указанный файл на диске.");
                if (!isOut)
                {
                    SetValuesAsNoDownloadedMode();
                }
            }
        }

        private void FileShowInFolder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (System.IO.File.Exists(this.filePath))
            {
                System.Diagnostics.Process.Start("explorer", "/select," + this.filePath);
            }
            else
            {
                MessageBox.Show("Файл не найден.");
                this.SetValuesAsNoDownloadedMode();
            }
        }

        #endregion

        #region Button Icon

        BitmapImage DownloadImage()
        {
            return new BitmapImage(new Uri("Images/Download.png", UriKind.RelativeOrAbsolute));
        }

        BitmapImage OpenImage()
        {
            return new BitmapImage(new Uri("Images/Open.png", UriKind.RelativeOrAbsolute));
        }

        BitmapImage CanselDownloading()
        {
            return new BitmapImage(new Uri("Images/Cansel.png", UriKind.RelativeOrAbsolute));
        }

        #endregion

        #region Animation

        void ShowDownloadingPanel()
        {
            Utility.UI.BeginHeightAnimation(this.grdDownloadingInfo, true, 195);
        }

        void HideErrorPanel()
        {
            Utility.UI.BeginHeightAnimation(this.ErrorDescription, false, 195);
        }

        void ShowErrorPanel()
        {
            Utility.UI.BeginHeightAnimation(this.ErrorDescription, true, 195);
        }

        void HideDownloadingPanel()
        {
            Utility.UI.BeginHeightAnimation(this.grdDownloadingInfo, false, 195);
        }

        #endregion
    }
}
