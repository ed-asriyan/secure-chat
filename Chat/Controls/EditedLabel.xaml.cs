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
    /// Interaction logic for EditedLabel.xaml
    /// </summary>
    public partial class EditedLabel : UserControl
    {
        #region Events

        public event TextChangedEventHandler TextChanged;

        #endregion

        #region Public values

        public string Text {
            get { return this.TextBox.Text; }
            set
            {
                this.TextBox.Text = value;
                TextBox_TextChanged(null, null);
            }
        }
        public double FontSize
        {
            get { return this.TextBlock.FontSize; }
            set { this.TextBlock.FontSize = value; }
        }
        public FontFamily FontFamily
        {
            get { return this.TextBlock.FontFamily; }
            set { this.TextBlock.FontFamily = value; }
        }

         TextBlock LabelElement
        {
            get { return this.TextBlock; }
            set { this.TextBlock = value; }
        }
          TextBox TextBoxElement
        {
            get { return this.TextBox; }
            set { this.TextBox = value; }
        }

        public bool CanEdit { get; set; }

        #endregion

        #region Constructors

        public EditedLabel()
        {
            InitializeComponent();

            this.Text = string.Empty;
            this.CanEdit = true;
            SetFocusOnLabel();

            this.TextBlock.MouseUp += Label_MouseUp;
            this.TextBox.LostFocus += TextBox_LostFocus;
            this.TextBox.KeyUp += TextBox_KeyUp;
            this.TextBox.TextChanged += TextBox_TextChanged;
            TextBox_TextChanged(this, null);
        }

        #endregion

        #region Routed events

        void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.TextBlock.Text = this.TextBox.Text;
            if (this.TextChanged != null)
            {
                this.TextChanged(this, e);
            }
            if (!string.IsNullOrEmpty(this.TextBox.Text))
            {
                this.TextBlock.Foreground = Brushes.Black;
            }
            else
            {
                this.TextBlock.Foreground = Brushes.Gray;
                this.TextBlock.Text = "пусто";
            }
            
        }

        void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (CanEdit)
            {
                if (e.Key == Key.Escape)
                {
                    this.TextBlock.Text = this.TextBox.Text;
                    SetFocusOnLabel();
                }
            }
        }

        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CanEdit)
            {
                SetFocusOnLabel();
            }
        }

        void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CanEdit)
            {
                SetFocusOnTextBox();
            }
        }

        #endregion

        #region Focus

        public void SetFocusOnLabel()
        {
            this.Dispatcher.Invoke(() =>
            {
              //  if (this.TextBox.Text == "") this.TextBlock.Text = "-";
                this.TextBlock.Visibility = System.Windows.Visibility.Visible;
                this.TextBox.Visibility = System.Windows.Visibility.Collapsed;
            });
        }
        public void SetFocusOnTextBox()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.TextBox.Visibility = System.Windows.Visibility.Visible;
                this.TextBlock.Visibility = System.Windows.Visibility.Collapsed;
                this.TextBox.Focus();
            });
        }

        #endregion
    }
}
