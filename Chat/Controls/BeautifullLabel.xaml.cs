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
    /// Interaction logic for BeautifullLabel.xaml
    /// </summary>
    public partial class BeautifullLabel : UserControl
    {
        #region Constructors

        public BeautifullLabel()
        {
            InitializeComponent();

            this.UpdateDuration = 200;
        }


        #endregion

        #region Public values

        public FontFamily FontFamily
        {
            get { return this.Label.FontFamily; }
            set { this.Label.FontFamily = value; }
        }

        public double FontSize
        {
            get { return this.Label.FontSize; }
            set { this.Label.FontSize = value; }
        }

        public Brush Foreground
        {
            get { return this.Label.Foreground; }
            set { this.Label.Foreground = value; }
        }

        public FontWeight FontWeight
        {
            set { this.Label.FontWeight = value; }
            get { return this.Label.FontWeight; }
        }

        public uint UpdateDuration { get; set; }
        public object Content
        {
            get { return this.Label.Content; }
            set { this.UpdateContent(value); }
        }

        #endregion

        #region Update content

        public void UpdateContent(object content)
        {
            try
            {
                if (content == null) throw new ArgumentNullException("text");

                if (this.Label.Content != null && this.Label.Content.GetHashCode() == content.GetHashCode()) return;

                TimeSpan duration = TimeSpan.FromMilliseconds(this.UpdateDuration / 2);

                double currentOpacity = 1;// this.Opacity;

                DoubleAnimation animFade = new DoubleAnimation(0, duration);
                DoubleAnimation animShow = new DoubleAnimation(currentOpacity, duration);

                animFade.Completed += (object sender, EventArgs e) =>
                {
                    this.Label.Content = content;
                    this.BeginAnimation(OpacityProperty, animShow);
                };

                this.BeginAnimation(OpacityProperty, animFade);
            }
            catch { }
        }

        #endregion
    }
}
