// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;
using System.Linq;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Avalonia.Controls
{
    /// <summary>
    /// A top-level menu control.
    /// </summary>
    public class Menu : SelectingItemsControl, IFocusScope, IMainMenu, IMenu
    {
        /// <summary>
        /// Defines the default items panel used by a <see cref="Menu"/>.
        /// </summary>
        private static readonly ITemplate<IPanel> DefaultPanel =
            new FuncTemplate<IPanel>(() => new StackPanel { Orientation = Orientation.Horizontal });

        /// <summary>
        /// Defines the <see cref="IsOpen"/> property.
        /// </summary>
        public static readonly DirectProperty<Menu, bool> IsOpenProperty =
            AvaloniaProperty.RegisterDirect<Menu, bool>(
                nameof(IsOpen),
                o => o.IsOpen);

        private readonly IMenuInteractionHandler _interaction;
        private bool _isOpen;

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        public Menu()
        {
            _interaction = AvaloniaLocator.Current.GetService<IMenuInteractionHandler>() ?? 
                new DefaultMenuInteractionHandler();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu"/> class.
        /// </summary>
        /// <param name="interactionHandler">The menu iteraction handler.</param>
        public Menu(IMenuInteractionHandler interactionHandler)
        {
            Contract.Requires<ArgumentNullException>(interactionHandler != null);

            _interaction = interactionHandler;
        }

        /// <summary>
        /// Initializes static members of the <see cref="Menu"/> class.
        /// </summary>
        static Menu()
        {
            ItemsPanelProperty.OverrideDefaultValue(typeof(Menu), DefaultPanel);
            MenuItem.SubmenuOpenedEvent.AddClassHandler<Menu>(x => x.OnSubmenuOpened);
        }

        /// <summary>
        /// Gets a value indicating whether the menu is open.
        /// </summary>
        public bool IsOpen
        {
            get { return _isOpen; }
            private set { SetAndRaise(IsOpenProperty, ref _isOpen, value); }
        }

        /// <inheritdoc/>
        IMenuInteractionHandler IMenu.InteractionHandler => _interaction;

        /// <inheritdoc/>
        IMenuItem IMenuElement.SelectedItem
        {
            get
            {
                var index = SelectedIndex;
                return (index != -1) ?
                    (IMenuItem)ItemContainerGenerator.ContainerFromIndex(index) :
                    null;
            }
            set
            {
                SelectedIndex = ItemContainerGenerator.IndexFromContainer(value);
            }
        }

        /// <summary>
        /// Closes the menu.
        /// </summary>
        public void Close()
        {
            foreach (MenuItem i in this.GetLogicalChildren())
            {
                i.IsSubMenuOpen = false;
            }

            IsOpen = false;
            SelectedIndex = -1;
        }

        /// <summary>
        /// Opens the menu in response to the Alt/F10 key.
        /// </summary>
        public void Open()
        {
            IsOpen = true;
            _interaction.MenuOpened(this);
        }

        /// <inheritdoc/>
        bool IMenuElement.MoveSelection(NavigationDirection direction, bool wrap) => MoveSelection(direction, wrap);

        /// <inheritdoc/>
        protected override IItemContainerGenerator CreateItemContainerGenerator()
        {
            return new ItemContainerGenerator<MenuItem>(this, MenuItem.HeaderProperty, null);
        }

        /// <inheritdoc/>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            var inputRoot = e.Root as IInputRoot;

            if (inputRoot?.AccessKeyHandler != null)
            {
                inputRoot.AccessKeyHandler.MainMenu = this;
            }

            _interaction.Attach(this);
        }

        /// <inheritdoc/>
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _interaction.Detach();
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            e.Handled = UpdateSelectionFromEventSource(e.Source, true);
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Source is MenuItem item)
            {
                _interaction.KeyDownEvent(item, e);
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            var item = GetMenuItem(e.Source as IControl);

            if (item != null)
            {
                _interaction.PointerEvent(item, e);
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var item = GetMenuItem(e.Source as IControl);

            if (item != null)
            {
                _interaction.PointerEvent(item, e);
            }
        }

        /// <summary>
        /// Called when the menu loses focus.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (e.Source == this)
            {
                SelectedItem = null;
            }
        }

        /// <summary>
        /// Called when a submenu opens somewhere in the menu.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnSubmenuOpened(RoutedEventArgs e)
        {
            if (e.Source is MenuItem menuItem && menuItem.Parent == this)
            {
                foreach (var child in this.GetLogicalChildren().OfType<MenuItem>())
                {
                    if (child != menuItem && child.IsSubMenuOpen)
                    {
                        child.IsSubMenuOpen = false;
                    }
                }
            }

            IsOpen = true;
        }

        private IMenuItem GetMenuItem(IControl item)
        {
            while (true)
            {
                if (item == null) return null;
                if (item is IMenuItem menuItem) return menuItem;
                item = item.Parent;
            }
        }
    }
}
