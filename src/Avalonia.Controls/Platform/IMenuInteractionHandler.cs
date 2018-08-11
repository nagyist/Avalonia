using System;
using Avalonia.Input;

namespace Avalonia.Controls.Platform
{
    public interface IMenuInteractionHandler
    {
        void Attach(IMenu menu);
        void Detach();
        void KeyDownEvent(IMenuItem item, KeyEventArgs e);
        void MenuOpened(IMenu menu);
        void PointerEvent(IMenuItem item, PointerEventArgs e);
    }
}
