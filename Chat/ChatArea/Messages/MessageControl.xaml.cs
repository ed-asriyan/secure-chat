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

using System.Windows.Media.Animation;
using System.Threading;

namespace Chat
{
    /// <summary>
    /// Interaction logic for MessageControl.xaml
    /// </summary>
    public partial class MessageControl : UserControl
    {
        #region Events

        public delegate void OnSelectDelegate(object sender, EventArgs e);
        public event OnSelectDelegate OnSelect;
        public event OnSelectDelegate OnUnselect;

        public event OnSelectDelegate OnSendAgain;

        public delegate void OnMessageDeleteDelegate(object sender, EventArgs e);
        public event OnMessageDeleteDelegate OnMessageDelete;

        #endregion

        bool Selected = false;

        #region Public values

        /// <summary>
        /// Прикреплённое сообщение
        /// </summary>
        public Message message { get; private set; }

        /// <summary>
        /// Прикреплённый чат
        /// </summary>
        public ChatBox Chatbox { get; private set; }

        /// <summary>
        /// Выделено ли сообщение
        /// </summary>
        public bool IsSelected
        {
            get { return this.Selected; }
            set
            {
                if (value)
                {
                    this.SelectMessage();
                }
                else
                {
                    this.UnselectMessage();
                }
            }
        }

        #endregion

        #region Constructors

        public MessageControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="mess">Сообщение</param>
        /// <param name="chatBox">Чат</param>
        public void Init(Message mess, ChatBox chatBox)
        {
           // mess.Out = false;
            this.message = mess;
            this.Chatbox = chatBox;

            if (!this.message.Out)
            {
                this.ProfilePhoto.Source = this.message.chatConnetcion.ProfilePhoto;
                this.ProfilePhoto.ToolTip = this.message.AutorName;
            }

            this.Body.Text = this.message.Body;
            if (string.IsNullOrWhiteSpace(mess.Body)) this.Body.Visibility = System.Windows.Visibility.Collapsed;

            if (mess.Out) SetAsOut();

            this.Time.Text = message.Time != null ? message.Time.ToShortTimeString() : DateTime.Now.ToShortTimeString();

            this.BubbleBorder.Background = new SolidColorBrush(Settings.Global.MessageBackgroundColor);

            if (this.message.Attachment != null)
            {
                AttachmentControl attachControl = new AttachmentControl();
                attachControl.Init(this.message.chatConnetcion, this.message.Attachment, this.message.Out);
                this.AttachmentsPanel.Children.Add(attachControl);

                Binding binding = new Binding();
                binding.ElementName = "this";
                binding.Path = new PropertyPath("ActualWidth");
                binding.Mode = BindingMode.OneWay;
                attachControl.SetBinding(MaxWidthProperty, binding);
            }


        }

        #endregion

        #region Delete message

        /// <summary>
        /// Удаляет сообщение
        /// </summary>
        public void Delete()
        {
            TimeSpan durationHeight = TimeSpan.FromMilliseconds(220);
            TimeSpan durationOpacity = TimeSpan.FromMilliseconds(200);

            this.Dispatcher.Invoke(() =>
            {
                DoubleAnimation animHeight = new DoubleAnimation(1, 0, durationHeight);
                DoubleAnimation animOpacity = new DoubleAnimation(1, 0, durationOpacity);

                this.Opacity = 0;
                animHeight.BeginTime = TimeSpan.FromMilliseconds(200);

                animHeight.DecelerationRatio = 0.75;

                var scaleTransform = new ScaleTransform();
                this.LayoutTransform = scaleTransform;

                animHeight.Completed += (object sender, EventArgs e) =>
                {
                    if (OnMessageDelete != null) OnMessageDelete(this, new EventArgs());
                };

                this.BeginAnimation(OpacityProperty, animOpacity);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animHeight);

            });
            
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Delete();
        }

        void animHeight_Completed(object sender, EventArgs e)
        {
            try
            {
                (this.Parent as Panel).Children.Remove(this);
            }
            catch { }
        }

        #endregion

        #region Select message

        /// <summary>
        /// Выделяет сообщение
        /// </summary>
        void SelectMessage()
        {
            if (this.Selected) return;
            this.Selected = true;
            this.Dispatcher.Invoke(() =>
            {
                imgSelect.Source = new BitmapImage(new Uri(@"Images/Checked.png", UriKind.RelativeOrAbsolute));
                if (OnSelect != null) OnSelect(this, new EventArgs());
                BeginAnimationSelect();
            });
        }

        void UnselectMessage()
        {
            if (!this.Selected) return;
            this.Selected = false;
            this.Dispatcher.Invoke(() =>
            {
                imgSelect.Source = new BitmapImage(new Uri(@"Images/Unchecked.png", UriKind.RelativeOrAbsolute));
                if (OnUnselect != null) OnUnselect(this, new EventArgs());
                BegionAnimationUnselect();
            });
        }

        private void MainBorder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Selected)
            {
                UnselectMessage();
            }
            else
            {
                SelectMessage();
            }
        }

        void BeginAnimationSelect()
        {
            var duration = TimeSpan.FromMilliseconds(70);

            bdgrWhiteLeft.SetCurrentValue(GradientStop.OffsetProperty, 0.0);
            bdgrColorLeft.SetCurrentValue(GradientStop.OffsetProperty, 0.0);
            bdgrWhiteRight.SetCurrentValue(GradientStop.OffsetProperty, 0.0);
            bdgrColorRight.SetCurrentValue(GradientStop.OffsetProperty, 0.0);
            DoubleAnimation animWhiteRight = new DoubleAnimation(0, 1, duration);
            DoubleAnimation animColorRight = new DoubleAnimation(0, 1, duration);
            animColorRight.BeginTime = TimeSpan.FromMilliseconds(35);

            bdgrWhiteRight.BeginAnimation(GradientStop.OffsetProperty, animWhiteRight);
            bdgrColorRight.BeginAnimation(GradientStop.OffsetProperty, animColorRight);

            DoubleAnimation anim = new DoubleAnimation(20, TimeSpan.FromMilliseconds(100));
            btnSelect.BeginAnimation(Viewbox.WidthProperty, anim);
        }

        void BegionAnimationUnselect()
        {
            var duration = TimeSpan.FromMilliseconds(70);
            DoubleAnimation animWhiteLeft = new DoubleAnimation(0, 1, duration);
            DoubleAnimation animColorLeft = new DoubleAnimation(0, 1, duration);
            animWhiteLeft.BeginTime = TimeSpan.FromMilliseconds(35);

            bdgrWhiteLeft.BeginAnimation(GradientStop.OffsetProperty, animWhiteLeft);
            bdgrColorLeft.BeginAnimation(GradientStop.OffsetProperty, animColorLeft);

            DoubleAnimation anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(70));
            btnSelect.BeginAnimation(Viewbox.WidthProperty, anim);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            MainBorder_MouseRightButtonUp(sender, null);
        }

        #endregion

        #region Animation

        void BeginHideErrorPanel()
        {
            Utility.UI.BeginHeightAndWidthAnimation(this.ErrorDescription, false, 195);
        }

        void BeginShowErrorPanel()
        {
            Utility.UI.BeginHeightAndWidthAnimation(this.ErrorDescription, true, 195);
        }

        public void BeginAnimationShow()
        {
            Utility.UI.BeginVisibilityAnimation(this, true, 220, 00);
            Utility.UI.BeginHeightAndWidthAnimation(this, true, 200);

        }

        public void ShowLoadingIcon()
        {
            Utility.UI.BeginVisibilityAnimation(this.Time, false, 100);
            Utility.UI.BeginVisibilityAnimation(this.SendingErrorIcon, false, 100);
            Utility.UI.BeginVisibilityAnimation(this.LoadingIcon, true, 100);
        }

        public void ShowTimeIcon()
        {
            Utility.UI.BeginVisibilityAnimation(this.Time, true, 100);
            Utility.UI.BeginVisibilityAnimation(this.SendingErrorIcon, false, 100);
            Utility.UI.BeginVisibilityAnimation(this.LoadingIcon, false, 100);
        }

        public void ShowErrorIcon()
        {
            Utility.UI.BeginVisibilityAnimation(this.Time, false, 100);
            Utility.UI.BeginVisibilityAnimation(this.SendingErrorIcon, true, 100);
            Utility.UI.BeginVisibilityAnimation(this.LoadingIcon, false, 100);
        }

        #endregion

        #region Error show

        public void ShowError(string error)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ErrorDescription.Text = error;
                BeginShowErrorPanel();
            }));
        }

        public void HideError()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ErrorDescription.Text = string.Empty;
                BeginHideErrorPanel();
            }));
        }
        private void SendingErrorIcon_Click(object sender, RoutedEventArgs e)
        {
            if (this.OnSendAgain != null)
            {
                this.OnSendAgain(sender, e);
            }
        }

        #endregion

        #region Internal

        void SetAsOut()
        {
            this.BubbleBorder.Background = new SolidColorBrush(Colors.Gray);
            this.ProfilePhoto.Visibility = System.Windows.Visibility.Collapsed;

            this.BubbleBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
        }

        #endregion
    }
}
