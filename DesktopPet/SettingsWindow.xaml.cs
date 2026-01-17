using System;
using System.Windows;
using Microsoft.Win32;

namespace DesktopPet
{
    public partial class SettingsWindow : Window
    {
        private MainWindow _main;
        private const string AppName = "MyDesktopPet";

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();
            _main = main;
            
            // Sync UI State
            RunOnStartupCheck.IsChecked = IsStartupEnabled();
            ScaleSlider.Value = _main.Width / 250.0; // Base size 250

            this.KeyDown += (s, e) => { if (e.Key == System.Windows.Input.Key.Escape) this.Close(); };
        }

        private void OnWindowDrag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void OnStartupChecked(object sender, RoutedEventArgs e)
        {
            SetStartup(true);
        }

        private void OnStartupUnchecked(object sender, RoutedEventArgs e)
        {
            SetStartup(false);
        }

        private void SetStartup(bool enable)
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, true))
            {
                if (key == null) return;
                if (enable)
                {
                    string? path = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                    if (path != null) key.SetValue(AppName, path);
                }
                else
                {
                    key.DeleteValue(AppName, false);
                }
            }
        }

        private bool IsStartupEnabled()
        {
            string runKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(runKey, false))
            {
                if (key == null) return false;
                return key.GetValue(AppName) != null;
            }
        }

        private void OnScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_main != null)
            {
                double baseSize = 250.0;
                double newSize = baseSize * e.NewValue;
                _main.Width = newSize;
                _main.Height = newSize;
                
                // Update digital display
                if (SizeLabel != null)
                {
                    SizeLabel.Text = $"{(int)(e.NewValue * 100)}%";
                }
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
