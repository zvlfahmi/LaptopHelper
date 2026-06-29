using LaptopHelper.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace LaptopHelper
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default titlebar
            this.SetTitleBar(titleBar); // Set the custom title bar

        }

        private void TitleBar_BackRequested(TitleBar sender, object args)
        {
            if (navFrame.CanGoBack)
            {
                navFrame.GoBack();
            }
        }
        private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            mainNav.IsPaneOpen = !mainNav.IsPaneOpen;
        }
        private void mainNavLoaded(object sender, RoutedEventArgs e)
        {
            if (mainNav.MenuItems.Count > 0)
            {
                mainNav.SelectedItem = mainNav.MenuItems[0];
            }
        }

        private void mainNavSel(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            // Use SelectedItemContainer if available; otherwise, cast SelectedItem directly
            var selectedItem = args.SelectedItemContainer ?? args.SelectedItem as NavigationViewItem;

            if (selectedItem != null)
            {
                string tag = selectedItem.Tag?.ToString() ?? string.Empty;
                Type? pageType = null;

                switch (tag)
                {
                    case "SysInfo":
                        pageType = typeof(SysInfo);
                        break;

                    case "BatMgt":
                        pageType = typeof(BatMgt);
                        break;

                    case "BiosSettings":
                        pageType = typeof(BiosSettings);
                        break;
                }

                if (pageType != null && navFrame.CurrentSourcePageType != pageType)
                {
                    navFrame.Navigate(pageType);
                }
            }
        }
    }
}