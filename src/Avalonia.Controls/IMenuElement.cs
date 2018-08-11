using System;
using Avalonia.Input;

namespace Avalonia.Controls
{
    /// <summary>
    /// Represents an <see cref="IMenu"/> or <see cref="IMenuItem"/>.
    /// </summary>
    public interface IMenuElement : IControl
    {
        IMenuItem SelectedItem { get; set; }
        void Open();
        void Close();
        bool MoveSelection(NavigationDirection direction, bool wrap);
    }
}
