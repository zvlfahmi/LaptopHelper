using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LaptopHelper.Models
{
    public class BiosSettingItem : INotifyPropertyChanged
    {
        private bool _isOn;
        private bool _isControlEnabled = true;
        private string _statusText = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    OnPropertyChanged();
                    StatusText = _isOn ? "Enabled" : "Disabled";
                }
            }
        }

        public bool IsControlEnabled
        {
            get => _isControlEnabled;
            set
            {
                if (_isControlEnabled != value)
                {
                    _isControlEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}