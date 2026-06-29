using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LaptopHelper.ViewModels;
using LaptopHelper.Models;

namespace LaptopHelper.Views
{
    public sealed partial class BiosSettings : Page
    {
        public BiosSettingsViewModel ViewModel { get; }

        public BiosSettings()
        {
            this.InitializeComponent();
            ViewModel = new BiosSettingsViewModel();
            this.Loaded += BiosSettings_Loaded;
        }

        private async void BiosSettings_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadSettingsAsync();
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggle && toggle.DataContext is BiosSettingItem item)
            {
                // Prevent duplicate trigger during initial loads or service rollbacks
                if (item.IsControlEnabled && item.IsOn != toggle.IsOn)
                {
                    await ViewModel.UpdateSettingAsync(item, toggle.IsOn);
                }
            }
        }

        // Helper methods for direct x:Bind visibility conversion
        public Visibility ConvertBoolToVisibility(bool value)
        {
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility ConvertBoolToInverseVisibility(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}