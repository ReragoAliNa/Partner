using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace DesktopPet
{
    public partial class GhostOverlayWindow : Window
    {
        public GhostOverlayWindow()
        {
            InitializeComponent();
            
            UpdateSize();
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += (s, e) => UpdateSize();
        }

        private void UpdateSize()
        {
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
        }

        public void SpawnGhost(ImageSource source, Rect bounds, double scaleX, double scaleY)
        {
            // Create the ghost image
            var ghost = new Image
            {
                Source = source,
                Width = bounds.Width,
                Height = bounds.Height,
                Opacity = 0.6,
                RenderTransformOrigin = new Point(0.5, 1.0) // Pivot at bottom center
            };

            // Apply the same scale/facing as the original
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(scaleX, scaleY));
            ghost.RenderTransform = transformGroup;

            // Position it
            Canvas.SetLeft(ghost, bounds.Left);
            Canvas.SetTop(ghost, bounds.Top);

            GhostCanvas.Children.Add(ghost);

            // Animate Opacity (Fade Out)
            var anim = new DoubleAnimation(0.6, 0.0, TimeSpan.FromMilliseconds(500));
            anim.Completed += (s, e) => 
            {
                GhostCanvas.Children.Remove(ghost);
            };
            
            ghost.BeginAnimation(OpacityProperty, anim);
            
            // Optional: Animate Scale (Shrink slightly?)
            // var scaleAnim = new DoubleAnimation(1.0, 0.8, TimeSpan.FromMilliseconds(500));
            // ... apply to ScaleTransform ...
        }
        
        public void ClearGhosts()
        {
            GhostCanvas.Children.Clear();
        }
    }
}
