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

namespace Chat
{
    /// <summary>
    /// Interaction logic for ChatControlTabHeader.xaml
    /// </summary>
    public partial class ChatControlTabHeader : UserControl
    {
        ChatBox chatBox; // chatbox, к которому привязан херер

        #region Values

        bool isSelected = false;
        /// <summary>
        /// Выделен ли хэдер
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetSelected(value);
            }
        }

        /// <summary>
        /// Название хэдэра
        /// </summary>
        public string HeadText
        {
            get
            {
                string result = "";
                this.Dispatcher.Invoke(new Action(() =>
                {
                    result = this.Head.Text;
                }));
                return result;
            }
            set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Head.Text = value;
                    if (this.isSelected) App.MainWindow.UpdateTitle(this.chatBox.ChatConnection);
                }));
            }
        }

        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string MessageText
        {
            get { return this.Message.Text; }
            private set
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Message.Text = value;
                }));
            }
        }

        #endregion

        #region Constructors

        public ChatControlTabHeader()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="chatBox">Привязанный ChatBox</param>
        public void Init(ChatBox chatBox)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.MessageText = chatBox.LastMessage != null ? chatBox.LastMessage.Body : "[ Нет сообщений ]";
                this.HeadText = chatBox.HeaderText;

                this.chatBox = chatBox;

                this.Update();
            }));
        }

        #endregion

        #region Update header's info

        /// <summary>
        /// Обновляет информацию
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void Update(Message message)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.HeadText = this.chatBox.ChatConnection.Name;

                string body = string.Empty;
                if (message == null) body = "[ Нет сообщений ]";
                else
                {
                    if (message.Out) body = "-> ";
                    if (string.IsNullOrWhiteSpace(message.Body) && message.Attachment != null) body += "[ Вложение ]";
                    else if (!string.IsNullOrWhiteSpace(message.Body))
                    {
                        body += message.Body;
                    }
                }
                this.MessageText = body;
            }));
        }

        /// <summary>
        /// Обновляет информацию
        /// </summary>
        public void Update()
        {
            Update(this.chatBox.LastMessage);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.ProfilePhoto.Source = this.chatBox.ChatConnection.ProfilePhoto;
            }));
        }

        #endregion

        #region Set typing

        /// <summary>
        /// Показывает уведомление о том, что пользователь начал набирать сообщение
        /// </summary>
        /// <param name="isTyping">Набирает ли пользоваетль сообщение</param>
        public void SetTyping(bool isTyping)
        {
            if (!isTyping)
            {
                Update();
            }
            else
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.MessageText = "Набирает сообщение...";
                }));
            }
        }

        #endregion

        #region Select header

        void SetSelected(bool selected)
        {
            if (isSelected == selected) return;
            this.isSelected = selected;
            var duration = TimeSpan.FromMilliseconds(100);
            if (selected)
            {
                BeginAnimationSelect();
            }
            else
            {
                BeginAnimationUnselect();
            }
        }

        #region Animation

        void BeginAnimationSelect()
        {
            DoubleAnimation animWhite = new DoubleAnimation(0, TimeSpan.FromMilliseconds(220));
            DoubleAnimation animTrans = new DoubleAnimation(0, TimeSpan.FromMilliseconds(150));
            ColorAnimation animGrayTr = new ColorAnimation(Colors.Transparent, TimeSpan.FromMilliseconds(200));
            animWhite.BeginTime = TimeSpan.FromMilliseconds(60);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.bdgrstWhite.BeginAnimation(GradientStop.OffsetProperty, animWhite);
                this.bdgrstTransparent.BeginAnimation(GradientStop.OffsetProperty, animTrans);
                this.bdgrstTransparent.BeginAnimation(GradientStop.ColorProperty, animGrayTr);
            }));
        }

        void BeginAnimationUnselect()
        {
            DoubleAnimation animWhite = new DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
            DoubleAnimation animTrans = new DoubleAnimation(1, TimeSpan.FromMilliseconds(220));
            animTrans.BeginTime = TimeSpan.FromMilliseconds(60);
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.bdgrstWhite.BeginAnimation(GradientStop.OffsetProperty, animWhite);
                this.bdgrstTransparent.BeginAnimation(GradientStop.OffsetProperty, animTrans);
            }));
        }

        #endregion

        private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.MainControlTabs.SetOnTop(this.chatBox);
        }

        #endregion

        #region Delete Chatbox

        private void btnRemoveChatBox_Click(object sender, RoutedEventArgs e)
        {
            Utility.UI.BeginVisibilityAnimation(this, false, 80, 0, 0);
            Utility.UI.BeginHeightAnimation(this.MainBorder, false, 200, 90, () => App.MainControlTabs.RemoveChatBox(this.chatBox));
        }

        #endregion

        #region New

        bool _isReaded = true;
        /// <summary>
        /// Устанавливает статус прочтения
        /// </summary>
        /// <param name="isReaded">Прочтено ли сообщение</param>
        public void SetReadState(bool isReaded)
        {
            if (isReaded == this._isReaded) return;
            this.Dispatcher.Invoke(() =>
            {
                this.recNewMessIndicator.Visibility = isReaded ? Visibility.Collapsed : Visibility.Visible;
                App.NotifyIcon.Icon = isReaded ? Properties.Resources.Icon : Properties.Resources.IconN;
                this._isReaded = isReaded;

                App.NotifyIcon.Text = MainWindow.PROGRAM_NAME + (isReaded ? "" : " (есть новые сообщения)");
            });
        }

        #endregion
    }
}
