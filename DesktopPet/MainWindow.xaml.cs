using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DesktopPet.Core;
using DesktopPet.States;

namespace DesktopPet
{
    public partial class MainWindow : Window
    {
        private PetContext _context = null!;
        private DateTime _lastFrameTime;
        private bool _isDragging = false;
        private Point _dragStartOffset;
        private GhostOverlayWindow _ghostOverlay = null!;
        private DateTime _lastGhostTime;
        private double _hoverTimer = 0;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set App Icon from Assets
            try {
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pet.png");
                if (System.IO.File.Exists(iconPath)) {
                    this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(iconPath));
                }
            } catch { /* Fallback to default if load fails */ }

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Initialize Core Logic
            _context = new PetContext(this);
            
            // Initial positioning (screen center)
            _context.Position = new Point(
                SystemParameters.PrimaryScreenWidth / 2 - Width / 2,
                SystemParameters.PrimaryScreenHeight / 2 - Height / 2
            );

            // Win32 Interop Setup
            var helper = new WindowInteropHelper(this);
            int exStyle = NativeMethods.GetWindowLong(helper.Handle, -20);
            NativeMethods.SetWindowLong(helper.Handle, -20, exStyle | NativeMethods.WS_EX_TOOLWINDOW);

            // Start Game Loop
            _lastFrameTime = DateTime.Now;
            CompositionTarget.Rendering += OnRendering;

            // Initialize Ghost Overlay
            _ghostOverlay = new GhostOverlayWindow();
            _ghostOverlay.Show();
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now - _lastFrameTime).TotalSeconds;
            _lastFrameTime = now;

            // Clamp delta time to avoid huge jumps if debugging or lag
            if (deltaTime > 0.1) deltaTime = 0.1;

            _context.Update(deltaTime);

            // Phase 2: Render
            var frame = _context.Animator.CurrentFrame;
            if (frame != null)
            {
                PetImage.Source = frame;
            }
            
            // Squash & Stretch Visual Application
            // Apply scale around the bottom-center of the image (pivot)
            PetImage.RenderTransformOrigin = new Point(0.5, 1.0);
            
            // Combine Physics Scale with Facing Direction
            double finalScaleX = _context.ScaleX * _context.FaceDirection;
            
            PetImage.RenderTransform = new ScaleTransform(finalScaleX, _context.ScaleY);

            // Ghost Trail Logic
            if (_isDragging && (DateTime.Now - _lastGhostTime).TotalMilliseconds > 40)
            {
                _lastGhostTime = DateTime.Now;
                 var bounds = new Rect(Left, Top, ActualWidth, ActualHeight);
                _ghostOverlay.SpawnGhost(PetImage.Source, bounds, finalScaleX, _context.ScaleY);
            }

            // Phase 3: Ghost Logic
            // Poll mouse position to handle click-through on transparent pixels
            ProcessGhostLogic(deltaTime);
        }


        private void ProcessGhostLogic(double deltaTime)
        {
            // 1. If dragging, always stay fully visible and interactive
            if (_isDragging)
            {
                PetImage.Opacity = 1.0;
                _hoverTimer = 0;
                ToggleWindowTransparency(false);
                return;
            }

            // 2. Check for CTRL key override (Force Interactive)
            bool isCtrlPressed = (NativeMethods.GetAsyncKeyState(NativeMethods.VK_CONTROL) & 0x8000) != 0;
            if (isCtrlPressed)
            {
                PetImage.Opacity = 1.0;
                _hoverTimer = 0;
                ToggleWindowTransparency(false);
                return;
            }

            // 3. Auto-Avoidance Logic (Smart Ghost Mode)
            NativeMethods.GetCursorPos(out var p);
            Point screenMouse = new Point(p.X, p.Y);
            
            // Check if mouse is inside window bounds
            bool isMouseOver = screenMouse.X >= Left && screenMouse.X <= Left + ActualWidth &&
                               screenMouse.Y >= Top && screenMouse.Y <= Top + ActualHeight;

            if (isMouseOver)
            {
                _hoverTimer += deltaTime;

                if (_hoverTimer > 0.8) // 800ms threshold
                {
                    // Enter "Avoidance Mode" - fade out and enable CLICK-THROUGH
                    // This allows the user to work on elements behind the pet
                    double fadeProgress = Math.Min(1.0, (_hoverTimer - 0.8) / 0.3); // Fade over 300ms
                    PetImage.Opacity = 1.0 - (fadeProgress * 0.7); // Fade to 0.3 (more transparent)
                    ToggleWindowTransparency(true);
                }
                else
                {
                    // Stay visible while waiting for threshold
                    PetImage.Opacity = 1.0;
                    ToggleWindowTransparency(false);
                }
            }
            else
            {
                // Mouse outside - Reset to solid
                _hoverTimer = 0;
                PetImage.Opacity = 1.0;
                ToggleWindowTransparency(false);
            }
        }

        private void ToggleWindowTransparency(bool transparent)
        {
             var helper = new WindowInteropHelper(this);
             int exStyle = NativeMethods.GetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE);
             bool currentlyTransparent = (exStyle & NativeMethods.WS_EX_TRANSPARENT) != 0;

             if (transparent && !currentlyTransparent)
             {
                 NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle | NativeMethods.WS_EX_TRANSPARENT);
             }
             else if (!transparent && currentlyTransparent)
             {
                 NativeMethods.SetWindowLong(helper.Handle, NativeMethods.GWL_EXSTYLE, exStyle & ~NativeMethods.WS_EX_TRANSPARENT);
             }
        }



        // Input Handling
        // Input Handling

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ButtonState == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _dragStartOffset = e.GetPosition(this); // Capture relative offset
                this.CaptureMouse(); // Important for manual drag

                // Explicitly tell the brain we are being dragged
                // Note: Position (0,0) is dummy, we handle move locally
                _context.TransitionTo(new DragState(_context, new Point(0,0)));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                NativeMethods.GetCursorPos(out var p);
                
                // DPI Awareness Fix:
                // NativeMethods.GetCursorPos returns physical pixels.
                // Window.Left/Top and _dragStartOffset are in Device Independent Pixels (DIPs).
                // We must convert physical mouse coordinates to DIPs using the window's DPI scale.
                var dpi = VisualTreeHelper.GetDpi(this);
                
                double mouseDipsX = p.X / dpi.DpiScaleX;
                double mouseDipsY = p.Y / dpi.DpiScaleY;

                // Update Window Position in DIPs
                Left = mouseDipsX - _dragStartOffset.X;
                Top = mouseDipsY - _dragStartOffset.Y;
                
                // Update Physics Position immediately
                if (_context != null)
                {
                     _context.Position = new Point(Left, Top);
                     _context.Velocity = new Vector(0,0); // Zero velocity while holding
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            
            if (_isDragging)
            {
                _isDragging = false;
                this.ReleaseMouseCapture();
                
                _context.HandleMouseUp(PointToScreen(e.GetPosition(this)));
            }
        }

        // Tray / Context Menu Actions
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            
            // Manually open the Context Menu attached to the Main Grid
            if (this.Content is Grid grid && grid.ContextMenu != null)
            {
                grid.ContextMenu.PlacementTarget = this;
                grid.ContextMenu.IsOpen = true;
            }
        }



        private void OnResetPosition(object sender, RoutedEventArgs e)
        {
             // Reset to center screen
             _context.Position = new Point(
                SystemParameters.PrimaryScreenWidth / 2 - Width / 2,
                SystemParameters.PrimaryScreenHeight / 2 - Height / 2
            );
            _context.Velocity = new Vector(0, 0);
            _context.TransitionTo(new FallState(_context));
        }

        private void OnOpenSettings(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(this);
            settings.ShowDialog();
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            _ghostOverlay?.Close();
            Application.Current.Shutdown();
        }
    }
}
