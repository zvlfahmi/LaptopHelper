using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LaptopHelper.Models;
using LaptopHelper.Services;
using Microsoft.UI.Dispatching;

namespace LaptopHelper.ViewModels
{
    public class BiosSettingsViewModel : INotifyPropertyChanged
    {
        private readonly BiosService _biosService;
        private readonly DispatcherQueue _dispatcherQueue;
        private bool _isLoading;

        public ObservableCollection<BiosSettingItem> Settings { get; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public BiosSettingsViewModel()
        {
            _biosService = new BiosService();
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        }

        public async Task LoadSettingsAsync()
        {
            IsLoading = true;
            Settings.Clear();

            await Task.Run(() =>
            {
                var data = BiosService.FetchBinarySettings();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var rawItem in data)
                    {
                        var item = new BiosSettingItem
                        {
                            Name = rawItem.Name,
                            IsOn = rawItem.CurrentValue.Equals("Enable", StringComparison.OrdinalIgnoreCase),
                            StatusText = rawItem.CurrentValue
                        };
                        Settings.Add(item);
                    }
                    IsLoading = false;
                });
            });
        }

        public async Task UpdateSettingAsync(BiosSettingItem item, bool newValue)
        {
            // Temporarily disable the switch during the write operation
            item.IsControlEnabled = false;

            string targetValue = newValue ? "Enable" : "Disable";

            bool success = await Task.Run(() =>
                _biosService.SetBiosSetting(item.Name, targetValue)
            );

            _dispatcherQueue.TryEnqueue(() =>
            {
                if (success)
                {
                    item.IsOn = newValue;
                }
                else
                {
                    // Revert the UI state if the BIOS operation failed
                    item.IsOn = !newValue;
                }
                item.IsControlEnabled = true;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}