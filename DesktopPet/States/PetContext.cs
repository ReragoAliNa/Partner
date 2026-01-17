using System;
using System.Windows;
using System.Windows.Media;
using DesktopPet.Core.Animation;

namespace DesktopPet.States
{
    public class PetContext
    {
        public Window PetWindow { get; private set; }
        public Vector Velocity { get; set; }
        public Point Position { get; set; }
        
        // Physics constants
        public const double Gravity = 2000.0; // Pixels per second squared
        public const double FloorLevel = 50.0; // Distance from bottom
        public const double TerminalVelocity = 1000.0;
        public const double DragFactor = 0.95; // Air resistance

        private IPetState _currentState;

        public AnimationController Animator { get; private set; }
        
        // "Living Object" Physics
        public bool EnableScalePhysics { get; set; } = true;
        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;
        public double TargetScaleX { get; set; } = 1.0;
        public double TargetScaleY { get; set; } = 1.0;
        public double FaceDirection { get; set; } = 1.0; // 1 = Right, -1 = Left
        private double _scaleVelocityX = 0;
        private double _scaleVelocityY = 0;

        public PetContext(Window window)
        {
            PetWindow = window;
            Animator = new AnimationController();
            
             // Try load Asset
            string assetPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "pet.png");
             if (!System.IO.File.Exists(assetPath))
            {
                 assetPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Assets/pet.png"));
            }

            if (System.IO.File.Exists(assetPath))
            {
                var idleAnim = AssetLoader.LoadSingleImage(assetPath);
                Animator.Play(idleAnim);
            }
            
            _currentState = new IdleState(this);
            _currentState.Enter();
            Position = new Point(window.Left, window.Top);
        }

        public void ApplyElasticImpulse(double force)
        {
            // Simple squash: wider X, shorter Y
            _scaleVelocityX += force;
            _scaleVelocityY -= force;
        }

        public void TransitionTo(IPetState newState)
        {
            _currentState.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update(double deltaTime)
        {
            _currentState.Update(deltaTime);
            Animator.Update(deltaTime);
            
            if (EnableScalePhysics)
            {
                // Update Spring Physics for Scale (Hooke's Law with Damping)
                double k = 150.0;
                double damping = 10.0;

                double displacementX = ScaleX - TargetScaleX;
                double forceX = -k * displacementX - damping * _scaleVelocityX;
                _scaleVelocityX += forceX * deltaTime;
                ScaleX += _scaleVelocityX * deltaTime;

                double displacementY = ScaleY - TargetScaleY;
                double forceY = -k * displacementY - damping * _scaleVelocityY;
                _scaleVelocityY += forceY * deltaTime;
                ScaleY += _scaleVelocityY * deltaTime;
            }
            
            // Sync Window Position - Optimization: Only set if changed
            if (Math.Abs(PetWindow.Left - Position.X) > 0.01)
                PetWindow.Left = Position.X;
                
            if (Math.Abs(PetWindow.Top - Position.Y) > 0.01)
                PetWindow.Top = Position.Y;
        }

        public void HandleMouseDown(Point p) => _currentState.OnMouseDown(p);
        public void HandleMouseUp(Point p) => _currentState.OnMouseUp(p);
    }
}
