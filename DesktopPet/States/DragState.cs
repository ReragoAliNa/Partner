using System.Windows;
using System.Windows.Input;

namespace DesktopPet.States
{
    public class DragState : StateBase
    {
        private Point _lastMousePos;
        private Point _offset; // Offset from window top-left to grab point

        public DragState(PetContext context, Point clickedPoint) : base(context)
        {
        }

        public override void Enter()
        {
            // Calculate offset relative to window position
            // But clickedPoint is likely internal or screen coordinates? 
            // We need to ensure we know where the mouse is relative to the pet.
            // For simplicity, we assume we grab the center or just track screen delta.
            
            Point screenMouse = GetScreenMouse();
            _lastMousePos = screenMouse;
            _offset = new Point(screenMouse.X - Context.Position.X, screenMouse.Y - Context.Position.Y);
            
            Context.Velocity = new Vector(0, 0); // Stop moving while dragged
            
            // Stretch Effect (Thin X, Tall Y) => Static Setting
            Context.EnableScalePhysics = false; // PAUSE PHYSICS
            Context.ScaleX = 0.9;
            Context.ScaleY = 1.1; 
        }

        public override void Update(double deltaTime)
        {
            Point currentMouse = GetScreenMouse();
            
            // Calculate instantaneous velocity for "throw" effect
            Vector delta = currentMouse - _lastMousePos;
            if (deltaTime > 0)
            {
                // Smooth velocity tracking could be better, but this is simple
                Context.Velocity = delta / deltaTime; 
            }
            
            _lastMousePos = currentMouse;
            
            // CRITICAL FIX: Do NOT set Context.Position here.
            // MainWindow calls DragMove(), which lets Windows OS handle the Move.
            // If we set Position here, we fight the OS, causing "jitter" or "offset" (错位).
            // Context.Position = new Point(currentMouse.X - _offset.X, currentMouse.Y - _offset.Y);
        }

        public override void OnMouseUp(Point mousePos)
        {
            Context.EnableScalePhysics = true; // RESUME PHYSICS
            Context.TargetScaleX = 1.0;
            Context.TargetScaleY = 1.0;
            Context.TransitionTo(new FallState(Context));
        }

        // Helper to get global mouse position
        private Point GetScreenMouse()
        {
            DesktopPet.Core.NativeMethods.GetCursorPos(out var p);
            return new Point(p.X, p.Y);
        }
    }
}
