using Microsoft.Management.Infrastructure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LaptopHelper.Views
{
    public sealed partial class BatMgt : Page
    {
        public BatMgt()
        {
            this.InitializeComponent();
            this.Loaded += BatMgt_Loaded;

        }

        private void BatMgt_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateBatteryInfo();
        }
        private void UpdateBatteryInfo()
        {
            CmbBatteryHealth.SelectionChanged -= CmbBatteryHealth_SelectionChanged;

            using var session = CimSession.Create(null);

            var bat = session.QueryInstances(@"root\cimv2", "WQL", "SELECT EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery").FirstOrDefault();

            if (bat != null)
            {
                TxtChargeRemaining.Text = bat.CimInstanceProperties["EstimatedChargeRemaining"].Value?.ToString() + "%";
                var status = bat.CimInstanceProperties["BatteryStatus"].Value?.ToString() ?? "N/A";
                TxtBatteryStatus.Text = TranslateBatteryStatus(status);
            }
            var bios = session.QueryInstances(@"root\hp\instrumentedbios", "WQL", "SELECT Value FROM HP_BIOSSetting WHERE Name='Battery Health Manager'").FirstOrDefault();
            string batManageHealth = bios?.CimInstanceProperties["Value"].Value?.ToString() ?? "Not Available";

            List<string> batManageHealthList = batManageHealth.Split(',').Select(x => x.Trim()).ToList();

            CmbBatteryHealth.ItemsSource = batManageHealthList.Select(x => x.TrimStart('*')).ToList();

            string activeSetting = batManageHealthList.FirstOrDefault(static x => x.StartsWith("*"));
            if (activeSetting != null)
            {
                CmbBatteryHealth.SelectedItem = activeSetting.TrimStart('*');
            }
            
            CmbBatteryHealth.SelectionChanged += CmbBatteryHealth_SelectionChanged;
        }

        private async void CmbBatteryHealth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem != null && e.AddedItems.Count > 0)
            {
                string selectedValue = comboBox.SelectedItem.ToString();
                comboBox.IsEnabled = false;

                try
                {
                    await Task.Run(() =>
                    {
                        using var session = CimSession.Create(null);
                        var interfaceInstance = session.QueryInstances(@"root\hp\InstrumentedBIOS", "WQL", "SELECT * FROM HP_BIOSSettingInterface").FirstOrDefault();

                        if (interfaceInstance != null)
                        {
                            var parameters = new CimMethodParametersCollection();
                            parameters.Add(CimMethodParameter.Create("Name", "Battery Health Manager", CimType.String, CimFlags.None));
                            parameters.Add(CimMethodParameter.Create("Value", selectedValue, CimType.String, CimFlags.None));
                            parameters.Add(CimMethodParameter.Create("Password", "<utf-16/>", CimType.String, CimFlags.None));

                            session.InvokeMethod(@"root\hp\InstrumentedBIOS", interfaceInstance, "SetBIOSSetting", parameters);
                        }
                    });

                    // Update UI on the main thread
                    this.DispatcherQueue.TryEnqueue(() => UpdateBatteryInfo());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to set BIOS: {ex.Message}");
                }
                finally
                {
                    comboBox.IsEnabled = true;
                }
            }
        }


        private string TranslateBatteryStatus(string status) => status switch
        {
            "1" => "Discharging",
            "2" => "On AC Power",
            "3" => "Fully Charged",
            "4" => "Low",
            "5" => "Critical",
            "6" => "Charging",
            _ => "Unknown"
        };
    }
}