using System;

namespace DesktopPet.States
{
    public class IdleState : StateBase
    {
        private double _timer;
        private double _duration;

        public IdleState(PetContext context) : base(context)
        {
        }

        public override void Enter()
        {
            Context.Velocity = new System.Windows.Vector(0, 0);
            _timer = 0;
            _duration = 5.0; // Idle for > 5 seconds before making a decision
        }

        public override void Update(double deltaTime)
        {
            _timer += deltaTime;

            // BREATHING ANIMATION
            // Sine wave: Amplitude 0.02 (2%), Frequency 2.0
            double breath = Math.Sin(_timer * 2.0) * 0.02;
            Context.TargetScaleY = 1.0 + breath;
            Context.TargetScaleX = 1.0 - (breath * 0.5); // Slight corrective squash

            if (_timer >= _duration)
            {
                // Reset scale before leaving
                Context.TargetScaleY = 1.0;
                Context.TargetScaleX = 1.0;

                // Attempt to release unused memory periodically when idle
                DesktopPet.Core.NativeMethods.FlushMemory();

                // Randomly choose next state
                if (Randomizer.NextDouble() > 0.3) // 70% chance to walk
                {
                    Context.TransitionTo(new WalkState(Context));
                }
                else
                {
                    // Reset idle with new duration
                    Enter();
                }
            }
            
            // Check if we are falling (e.g. if screen resolution changed or logic moved us)
            if (!IsOnFloor())
            {
                Context.TransitionTo(new FallState(Context));
            }
        }
    }
}
