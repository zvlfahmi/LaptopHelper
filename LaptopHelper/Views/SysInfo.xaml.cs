using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Microsoft.Management.Infrastructure;
using System.Threading.Tasks;

namespace LaptopHelper.Views
{
    public sealed partial class SysInfo : Page
    {
        public SysInfo()
        {
            this.InitializeComponent();
            this.Loaded += SysInfo_Loaded;
        }

        private async void SysInfo_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSystemInformationAsync();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadSystemInformationAsync();
        }

        private async Task LoadSystemInformationAsync()
        {
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;
            ContentScrollViewer.Visibility = Visibility.Collapsed;
            RefreshButton.IsEnabled = false;

            // Run CIM queries on a background thread to prevent UI freezing
            var info = await Task.Run(() => FetchCimData());

            // Populate the UI
            TxtProductName.Text = info.ProductName;
            TxtSerialNumber.Text = info.SerialNumber;
            TxtSkuNumber.Text = info.SkuNumber;
            TxtBoardId.Text = info.BoardId;
            TxtMemorySize.Text = info.MemorySize;
            TxtProcessorType.Text = info.ProcessorType;
            TxtProcessorSpeed.Text = info.ProcessorSpeed;
            TxtUuid.Text = info.Uuid;
            TxtUuidStandard.Text = info.UuidStandard;
            TxtAssetTag.Text = info.AssetTag;
            TxtOwnershipTag.Text = info.OwnershipTag;
            TxtBiosVersion.Text = info.BiosVersion;

            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
            ContentScrollViewer.Visibility = Visibility.Visible;
            RefreshButton.IsEnabled = true;
        }

        private SystemDetails FetchCimData()
        {
            var details = new SystemDetails();

            // Product Name
            details.ProductName = GetCimValue("Win32_ComputerSystem", "Model");

            // Serial Number
            details.SerialNumber = GetCimValue("Win32_BIOS", "SerialNumber");

            // SKU Number
            details.SkuNumber = GetCimValue("Win32_ComputerSystem", "SystemSKUNumber");
            if (details.SkuNumber == "Not Available")
            {
                details.SkuNumber = GetCimValue("Win32_ComputerSystemProduct", "SKUNumber");
            }

            // System Board ID
            details.BoardId = GetCimValue("Win32_BaseBoard", "Product");

            // Memory Size
            string rawMemory = GetCimValue("Win32_ComputerSystem", "TotalPhysicalMemory");
            if (double.TryParse(rawMemory, out double bytes))
            {
                double gb = bytes / (1024 * 1024 * 1024);
                details.MemorySize = $"{Math.Round(gb, 2)} GB";
            }
            else
            {
                details.MemorySize = "Not Available";
            }

            // Processor Name & Speed
            details.ProcessorType = GetCimValue("Win32_Processor", "Name");
            string rawSpeed = GetCimValue("Win32_Processor", "MaxClockSpeed");
            if (double.TryParse(rawSpeed, out double mhz))
            {
                details.ProcessorSpeed = $"{Math.Round(mhz / 1000, 2)} GHz";
            }
            else
            {
                details.ProcessorSpeed = "Not Available";
            }

            // UUID
            string rawUuid = GetCimValue("Win32_ComputerSystemProduct", "UUID");
            details.Uuid = rawUuid;

            if (Guid.TryParse(rawUuid, out Guid parsedGuid))
            {
                details.UuidStandard = parsedGuid.ToString("D").ToUpper();
            }
            else
            {
                details.UuidStandard = "Not Available";
            }

            // Asset Tracking & Ownership
            details.AssetTag = GetCimValue("Win32_SystemEnclosure", "AssetTag");
            details.OwnershipTag = GetCimValue("Win32_SystemEnclosure", "SMBIOSAssetTag");

            // Primary Battery Serial
            details.BatterySerial = GetCimValue("Win32_Battery", "SerialNumber");

            // System BIOS Version
            details.BiosVersion = GetCimValue("Win32_BIOS", "SMBIOSBIOSVersion");

            return details;
        }

        private string GetCimValue(string cimClass, string property)
        {
            try
            {
                using (var session = CimSession.Create(null))
                {
                    var instances = session.QueryInstances(@"root\cimv2", "WQL", $"SELECT {property} FROM {cimClass}");
                    foreach (var instance in instances)
                    {
                        var val = instance.CimInstanceProperties[property]?.Value?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(val)) return val;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CIM Query Failed for {cimClass}.{property} | Error: {ex.Message}");
            }
            return "Not Available";
        }
    }

    public class SystemDetails
    {
        public string ProductName { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string SkuNumber { get; set; } = string.Empty;
        public string BoardId { get; set; } = string.Empty;
        public string MemorySize { get; set; } = string.Empty;
        public string ProcessorType { get; set; } = string.Empty;
        public string ProcessorSpeed { get; set; } = string.Empty;
        public string Uuid { get; set; } = string.Empty;
        public string UuidStandard { get; set; } = string.Empty;
        public string AssetTag { get; set; } = string.Empty;
        public string OwnershipTag { get; set; } = string.Empty;
        public string BatterySerial { get; set; } = string.Empty;
        public string BiosVersion { get; set; } = string.Empty;
    }
}