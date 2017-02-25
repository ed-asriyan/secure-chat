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
    /// Interaction logic for LoadingIcon.xaml
    /// </summary>
    public partial class LoadingIcon : UserControl
    {
        #region Public values

        public Brush Brush
        {
            get
            {
                return this.ellipse.Stroke;
            }
            set
            {
                this.ellipse.Stroke = value;
            }
        }

        public double Thickness
        {
            get
            {
                return this.ellipse.StrokeThickness;
            }
            set
            {
                this.ellipse.StrokeThickness = value;
            }
        }


        #endregion

        #region Animation

        void BeginAnimationRotate()
        {
            DoubleAnimation anim = new DoubleAnimation(359, TimeSpan.FromMilliseconds(900));
            RotateTransform rt = new RotateTransform();

            this.ellipse.RenderTransform = rt;
            this.ellipse.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            anim.RepeatBehavior = RepeatBehavior.Forever;

            rt.BeginAnimation(RotateTransform.AngleProperty, anim);
        }

        void StopAnimationRotate()
        {
            if (this.ellipse.RenderTransform != null && this.ellipse.RenderTransform is RotateTransform)
            {
                var rt = this.ellipse.RenderTransform as RotateTransform;
                rt.BeginAnimation(RotateTransform.AngleProperty, null);
            }
        }

        #endregion

        #region Constructors

        public LoadingIcon()
        {
            InitializeComponent();

            BeginAnimationRotate();
        }

        #endregion

        #region Routed events

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                this.BeginAnimationRotate();
            }
            else
            {
                this.StopAnimationRotate();
            }
        }

        #endregion
    }
}
