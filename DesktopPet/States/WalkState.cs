using System;
using System.Windows;

namespace DesktopPet.States
{
    public class WalkState : StateBase
    {
        private double _direction; // 1 for right, -1 for left
        private double _duration;
        private double _timer;
        private const double Speed = 150.0;

        public WalkState(PetContext context) : base(context)
        {
        }

        public override void Enter()
        {
            _direction = Randomizer.NextDouble() > 0.5 ? 1.0 : -1.0;
            _duration = Randomizer.NextDouble() * 3 + 2; // 2 to 5 seconds
            Context.Velocity = new Vector(Speed * _direction, 0);
        }

        public override void Update(double deltaTime)
        {
            _timer += deltaTime;
            
            // Move
            Context.Position += Context.Velocity * deltaTime;

            // Check bounds - turn around if hitting edge
            double screenWidth = SystemParameters.WorkArea.Width;
            if (Context.Position.X <= 0)
            {
                Context.Position = new Point(0, Context.Position.Y);
                _direction = 1;
                Context.Velocity = new Vector(Speed * _direction, 0);
            }
            else if (Context.Position.X + Context.PetWindow.Width >= screenWidth)
            {
                Context.Position = new Point(screenWidth - Context.PetWindow.Width, Context.Position.Y);
                _direction = -1;
                Context.Velocity = new Vector(Speed * _direction, 0);
            }

            // BOBBING ANIMATION (Walk Cycle)
            // Faster and more pronounced than breathing
            double bob = Math.Abs(Math.Sin(_timer * 10.0)) * 0.05; // 0.0 to 0.05
            Context.TargetScaleY = 1.0 - bob; // Slight squash on every step
            Context.TargetScaleX = 1.0 + (bob * 0.5);

            // Flip visual based on direction
            // We assume original sprite faces RIGHT.
            if (_direction < 0)
            {
                // Flip X
                Context.FaceDirection = -1.0;
            }
            else
            {
                // Reset
                Context.FaceDirection = 1.0;
            }

            if (_timer >= _duration)
            {
                Context.TargetScaleY = 1.0;
                Context.TargetScaleX = 1.0;
                Context.TransitionTo(new IdleState(Context));
            }

            if (!IsOnFloor())
            {
                Context.TransitionTo(new FallState(Context));
            }
        }
    }
}
