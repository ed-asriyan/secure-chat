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
    /// Interaction logic for ErrorIcon.xaml
    /// </summary>
    public partial class ErrorIcon : UserControl
    {
        public object Description
        {
            get
            {
                object result = null;
                this.Dispatcher.Invoke(() =>
                {
                    result = this.MainImage.ToolTip;
                });
                return result;
            }
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.MainImage.ToolTip = value;
                });
            }
        }

        public ErrorIcon()
        {
            InitializeComponent();
        }
    }
}
