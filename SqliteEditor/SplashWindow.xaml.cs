using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace SqliteEditor
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            // start a subtle indeterminate animation for progress bar
            Loaded += (s, e) => StartProgressAnimation();
        }

        private void StartProgressAnimation()
        {
            var anim = new DoubleAnimation
            {
                From = 0,
                To = 240,
                Duration = TimeSpan.FromSeconds(1.2),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            ProgressBar.BeginAnimation(WidthProperty, anim);
        }

        public void FadeOutAndClose()
        {
            var fade = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(300)));
            fade.Completed += (s, e) => this.Close();
            this.BeginAnimation(OpacityProperty, fade);
        }
    }
}
