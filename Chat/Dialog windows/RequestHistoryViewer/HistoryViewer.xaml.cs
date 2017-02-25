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

namespace Chat
{
    /// <summary>
    /// Interaction logic for HistoryViewer.xaml
    /// </summary>
    public partial class HistoryViewer : Window
    {
        #region Constructors

        public HistoryViewer()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialization

        public void Init()
        {
            this.Update();
        }

        #endregion

        #region Updating

        public void Update()
        {
            this.Dispatcher.Invoke(() =>
            {
                bool scrollToBottomIsNeeded = this.Scroller.ScrollableHeight == this.Scroller.VerticalOffset;
                bool scrollToBottomIsNeeded1 = this.Scroller1.ScrollableHeight == this.Scroller1.VerticalOffset;

                this.txtblLog.Items.Clear();
                this.txtblLog1.Items.Clear();
                
                var _base = RequestHistoryBase.Inbox.Get();
                var _base1 = RequestHistoryBase.Outbox.Get();

                foreach (var element in _base)
                {
                    this.txtblLog.Items.Add(element.ToString());
                }

                foreach (var element in _base1)
                {
                    this.txtblLog1.Items.Add(element.ToString());
                }

                this.UpdateTime.Text = "Обновлено в " + DateTime.Now.ToShortTimeString();

                if (scrollToBottomIsNeeded) this.Scroller.ScrollToBottom();
                if (scrollToBottomIsNeeded1) this.Scroller1.ScrollToBottom();
            });
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Update();
        }

        #endregion
    }
}
