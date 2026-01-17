using System;
using System.Windows;

namespace DesktopPet.States
{
    public abstract class StateBase : IPetState
    {
        protected PetContext Context;
        protected Random Randomizer = new Random();

        protected StateBase(PetContext context)
        {
            Context = context;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(double deltaTime) { }

        public virtual void OnMouseDown(Point mousePos)
        {
            Context.TransitionTo(new DragState(Context, mousePos));
        }

        public virtual void OnMouseUp(Point mousePos) { }

        protected bool IsOnFloor()
        {
            double screenHeight = SystemParameters.WorkArea.Height;
            // Assuming Top-Left pivot, check Bottom
            return (Context.Position.Y + Context.PetWindow.Height) >= screenHeight;
        }

        protected void KeepOnScreen()
        {
             double screenWidth = SystemParameters.WorkArea.Width;
             double screenHeight = SystemParameters.WorkArea.Height;
             
             if (Context.Position.X < 0) Context.Position = new Point(0, Context.Position.Y);
             if (Context.Position.X > screenWidth - Context.PetWindow.Width) 
                Context.Position = new Point(screenWidth - Context.PetWindow.Width, Context.Position.Y);
                
             if (Context.Position.Y > screenHeight - Context.PetWindow.Height)
                Context.Position = new Point(Context.Position.X, screenHeight - Context.PetWindow.Height);
        }
    }
}
