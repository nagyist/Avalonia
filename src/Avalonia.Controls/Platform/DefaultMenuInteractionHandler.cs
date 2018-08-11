using System;
using Avalonia.Input;

namespace Avalonia.Controls.Platform
{
    public class DefaultMenuInteractionHandler : IMenuInteractionHandler
    {
        public void Attach(IMenu menu)
        {
        }

        public void Detach()
        {
        }

        public void KeyDownEvent(IMenuItem item, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    if (item.IsTopLevel)
                    {
                        if (item.HasSubMenu && !item.IsSubMenuOpen)
                        {
                            Open(item);
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        goto default;
                    }
                    break;

                case Key.Left:
                    if (item?.Parent is IMenuItem parent && !parent.IsTopLevel && parent.IsSubMenuOpen)
                    {
                        parent.Close();
                        parent.Focus();
                        e.Handled = true;
                    }
                    else
                    {
                        goto default;
                    }
                    break;

                case Key.Right:
                    if (!item.IsTopLevel && item.HasSubMenu)
                    {
                        Open(item);
                        e.Handled = true;
                    }
                    else
                    {
                        goto default;
                    }
                    break;

                case Key.Enter:
                    if (!item.HasSubMenu)
                    {
                        Click(item);
                    }
                    else
                    {
                        Open(item);
                    }

                    e.Handled = true;
                    break;

                case Key.Escape:
                    if (item.Parent != null)
                    {
                        item.Parent.Close();
                        item.Parent.Focus();
                        e.Handled = true;
                    }
                    break;

                default:
                    var direction = e.Key.ToNavigationDirection();

                    if (direction.HasValue)
                    {
                        if (item.Parent?.MoveSelection(direction.Value, true) == true)
                        {
                            // If the the parent is an IMenu which successfully moved its selection,
                            // and the current menu is open then close the current menu and open the
                            // new menu.
                            if (item.IsSubMenuOpen && item.Parent is IMenu)
                            {
                                item.Close();
                                Open(item.Parent.SelectedItem);
                            }
                            e.Handled = true;
                        }
                    }

                    break;
            }

            if (!e.Handled && item.Parent is IMenuItem parentItem)
            {
                KeyDownEvent(parentItem, e);
            }
        }

        public void MenuOpened(IMenu menu)
        {
            menu.MoveSelection(NavigationDirection.First, true);
        }

        public void PointerEvent(IMenuItem item, PointerEventArgs e)
        {
            if (e.RoutedEvent == InputElement.PointerPressedEvent &&
                e is PointerPressedEventArgs pressed &&
                pressed.MouseButton == MouseButton.Left &&
                item.HasSubMenu)
            {
                Open(item);
                e.Handled = true;
            }
            else if (e.RoutedEvent == InputElement.PointerReleasedEvent &&
                e is PointerReleasedEventArgs released &&
                released.MouseButton == MouseButton.Left &&
                !item.HasSubMenu)
            {
                Click(item);
                e.Handled = true;
            }
            else if (e.RoutedEvent == InputElement.PointerEnterEvent &&
                item.Parent != null)
            {
                item.Parent.SelectedItem = item;
                e.Handled = true;
            }
        }

        private void Click(IMenuItem item)
        {
            item.RaiseClick();
            CloseMenu(item);
        }

        private void CloseMenu(IMenuItem item)
        {
            var current = (IMenuElement)item;

            while (current != null && !(current is IMenu))
            {
                current = (current as IMenuItem).Parent;
            }

            current?.Close();
        }

        private void Open(IMenuItem item)
        {
            item.Open();
            item.MoveSelection(NavigationDirection.First, true);
        }
    }
}
