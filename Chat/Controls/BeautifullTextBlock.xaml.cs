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
    /// Interaction logic for BeautifulTextBlock.xaml
    /// </summary>
    public partial class BeautifullTextBlock : UserControl
    {
        #region Constructors

        public BeautifullTextBlock()
        {
            InitializeComponent();

            this.UpdateDuration = 200;
        }

        #endregion

        #region Public values

        public TextWrapping TextWrapping
        {
            get { return this.TextBlock.TextWrapping; }
            set { this.TextBlock.TextWrapping = value; }
        }

        public uint UpdateDuration { get; set; }
        public string Text
        {
            get { return this.TextBlock.Text; }
            set { this.UpdateText(value); }
        }

        public TextAlignment TextAlignment
        {
            get { return this.TextBlock.TextAlignment; }
            set { this.TextBlock.TextAlignment = value; }
        }

        #endregion

        #region Update text

        public void UpdateText(string text)
        {
            if (text == null)
            {
              //  throw new ArgumentNullException("text");
                return;
            }

            if (this.TextBlock.Text == text) return;

            TimeSpan duration = TimeSpan.FromMilliseconds(this.UpdateDuration / 2);

            double currentOpacity = 1;//this.Opacity;

            DoubleAnimation animFade = new DoubleAnimation(0, duration);
            DoubleAnimation animShow = new DoubleAnimation(currentOpacity, duration);

            animFade.Completed += (object sender, EventArgs e) =>
            {
                this.TextBlock.Text = text;
                this.BeginAnimation(OpacityProperty, animShow);
            };

            this.BeginAnimation(OpacityProperty, animFade);
        }

        #endregion
    }
}
