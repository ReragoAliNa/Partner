using System.Windows;

namespace DesktopPet.States
{
    public interface IPetState
    {
        void Enter();
        void Update(double deltaTime);
        void Exit();
        void OnMouseDown(Point mousePos);
        void OnMouseUp(Point mousePos);
    }
}
