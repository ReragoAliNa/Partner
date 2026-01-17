using System;
using System.Windows;

namespace DesktopPet.States
{
    public class FallState : StateBase
    {
        public FallState(PetContext context) : base(context) { }

        public override void Update(double deltaTime)
        {
            // Apply Gravity
            double newYVelocity = Context.Velocity.Y + (PetContext.Gravity * deltaTime);
            Context.Velocity = new Vector(Context.Velocity.X, newYVelocity);

            // Apply Velocity to Position
            Point newPos = Context.Position + (Context.Velocity * deltaTime);
            Context.Position = newPos;

            // Check Floor Collision
            if (IsOnFloor())
            {
                // Simple bounce/stop
                if (Math.Abs(Context.Velocity.Y) > 500) // fast impact
                {
                     Context.Velocity = new Vector(Context.Velocity.X * 0.5, -Context.Velocity.Y * 0.3); // Bounce
                     Context.ApplyElasticImpulse(5.0); // Big Squash
                }
                else
                {
                    Context.ApplyElasticImpulse(2.0); // Small Squash
                    Context.TransitionTo(new IdleState(Context));
                }
            }
            
            KeepOnScreen();
        }
    }
}
