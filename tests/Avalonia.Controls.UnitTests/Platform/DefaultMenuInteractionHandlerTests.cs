using System;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Moq;
using Xunit;

namespace Avalonia.Controls.UnitTests.Platform
{
    public class DefaultMenuInteractionHandlerTests
    {
        public class TopLevel
        {
            [Fact]
            public void Up_Opens_MenuItem_With_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var e = new KeyEventArgs { Key = Key.Up };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Down_Opens_MenuItem_With_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var e = new KeyEventArgs { Key = Key.Down };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Right_Selects_Next_MenuItem()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>(x => x.MoveSelection(NavigationDirection.Right, true) == true);
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.Parent == menu);
                var e = new KeyEventArgs { Key = Key.Right };

                target.KeyDownEvent(item, e);

                Mock.Get(menu).Verify(x => x.MoveSelection(NavigationDirection.Right, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Left_Selects_Previous_MenuItem()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>(x => x.MoveSelection(NavigationDirection.Left, true) == true);
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.Parent == menu);
                var e = new KeyEventArgs { Key = Key.Left };

                target.KeyDownEvent(item, e);

                Mock.Get(menu).Verify(x => x.MoveSelection(NavigationDirection.Left, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Enter_On_Item_With_No_SubMenu_Causes_Click()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.Parent == menu);
                var e = new KeyEventArgs { Key = Key.Enter };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.RaiseClick());
                Mock.Get(menu).Verify(x => x.Close());
                Assert.True(e.Handled);
            }

            [Fact]
            public void Enter_On_Item_With_SubMenu_Opens_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true && x.Parent == menu);
                var e = new KeyEventArgs { Key = Key.Enter };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Escape_Closes_Parent_Menu()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var item = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.Parent == menu);
                var e = new KeyEventArgs { Key = Key.Escape };

                target.KeyDownEvent(item, e);

                Mock.Get(menu).Verify(x => x.Close());
                Assert.True(e.Handled);
            }
        }

        public class NonTopLevel
        {
            [Fact]
            public void Up_Selects_Previous_MenuItem()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Up };

                target.KeyDownEvent(item, e);

                Mock.Get(parentItem).Verify(x => x.MoveSelection(NavigationDirection.Up, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Down_Selects_Next_MenuItem()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Down };

                target.KeyDownEvent(item, e);

                Mock.Get(parentItem).Verify(x => x.MoveSelection(NavigationDirection.Down, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Left_Closes_Parent_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.HasSubMenu == true && x.IsSubMenuOpen == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Left };

                target.KeyDownEvent(item, e);
                
                Mock.Get(parentItem).Verify(x => x.Close());
                Mock.Get(parentItem).Verify(x => x.Focus());
                Assert.True(e.Handled);
            }

            [Fact]
            public void Right_With_SubMenu_Items_Opens_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem && x.HasSubMenu == true);
                var e = new KeyEventArgs { Key = Key.Right };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Right_On_TopLevel_Child_Navigates_TopLevel_Selection()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = new Mock<IMenu>();
                var parentItem = Mock.Of<IMenuItem>(x => 
                    x.IsSubMenuOpen == true &&
                    x.IsTopLevel == true && 
                    x.HasSubMenu == true && 
                    x.Parent == menu.Object);
                var nextItem = Mock.Of<IMenuItem>(x =>
                    x.IsTopLevel == true &&
                    x.HasSubMenu == true &&
                    x.Parent == menu.Object);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Right };

                menu.Setup(x => x.MoveSelection(NavigationDirection.Right, true))
                    .Callback(() => menu.SetupGet(x => x.SelectedItem).Returns(nextItem))
                    .Returns(true);

                target.KeyDownEvent(item, e);

                menu.Verify(x => x.MoveSelection(NavigationDirection.Right, true));
                Mock.Get(parentItem).Verify(x => x.Close());
                Mock.Get(nextItem).Verify(x => x.Open());
                Mock.Get(nextItem).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Enter_On_Item_With_No_SubMenu_Causes_Click()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true && x.Parent == menu);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Enter };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.RaiseClick());
                Mock.Get(menu).Verify(x => x.Close());
                Assert.True(e.Handled);
            }

            [Fact]
            public void Enter_On_Item_With_SubMenu_Opens_SubMenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem && x.HasSubMenu == true);
                var e = new KeyEventArgs { Key = Key.Enter };

                target.KeyDownEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }

            [Fact]
            public void Escape_Closes_Parent_MenuItem()
            {
                var target = new DefaultMenuInteractionHandler();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new KeyEventArgs { Key = Key.Escape };

                target.KeyDownEvent(item, e);

                Mock.Get(parentItem).Verify(x => x.Close());
                Mock.Get(parentItem).Verify(x => x.Focus());
                Assert.True(e.Handled);
            }

            [Fact]
            public void PointerEnter_Selects_Item()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true && x.Parent == menu);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new PointerEventArgs { RoutedEvent = InputElement.PointerEnterEvent };

                target.PointerEvent(item, e);

                Mock.Get(parentItem).VerifySet(x => x.SelectedItem = item);
                Assert.True(e.Handled);
            }

            [Fact]
            public void PointerReleased_On_Item_With_No_SubMenu_Causes_Click()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true && x.Parent == menu);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem);
                var e = new PointerReleasedEventArgs { MouseButton = MouseButton.Left };

                target.PointerEvent(item, e);

                Mock.Get(item).Verify(x => x.RaiseClick());
                Mock.Get(menu).Verify(x => x.Close());
                Assert.True(e.Handled);
            }

            [Fact]
            public void PointerPressed_On_Item_With_SubMenu_Causes_Opens_Submenu()
            {
                var target = new DefaultMenuInteractionHandler();
                var menu = Mock.Of<IMenu>();
                var parentItem = Mock.Of<IMenuItem>(x => x.IsTopLevel == true && x.HasSubMenu == true && x.Parent == menu);
                var item = Mock.Of<IMenuItem>(x => x.Parent == parentItem && x.HasSubMenu == true);
                var e = new PointerPressedEventArgs { MouseButton = MouseButton.Left };

                target.PointerEvent(item, e);

                Mock.Get(item).Verify(x => x.Open());
                Mock.Get(item).Verify(x => x.MoveSelection(NavigationDirection.First, true));
                Assert.True(e.Handled);
            }
        }
    }
}
