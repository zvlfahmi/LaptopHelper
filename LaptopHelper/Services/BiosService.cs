using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Management.Infrastructure;

namespace LaptopHelper.Services
{
    public class BiosService
    {
        private const string Namespace = @"root\hp\InstrumentedBIOS";

        public static List<BiosSettingData> FetchBinarySettings()
        {
            var settings = new List<BiosSettingData>();

            var targetSettings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Turbo-boost",
                "Hyperthreading",
                "Power On When Lid is Opened",
                "Wake on AC",
                "Virtualization Technology (VTx)",
                "Virtualization Technology for Directed I/O (VTd)",
                "Fast Boot",
                "Fan Always on while on AC Power",
                "Fast Charge",
                "Swap Fn and Ctrl (Keys)",
                "USB Legacy Port Charging",
                "NumLock on at boot"
            };

            try
            {
                using var session = CimSession.Create(null);
                var instances = session.QueryInstances(Namespace, "WQL", "SELECT Name, Value FROM HP_BIOSSetting");

                foreach (var instance in instances)
                {
                    string name = instance.CimInstanceProperties["Name"].Value?.ToString() ?? string.Empty;

                    if (targetSettings.Contains(name))
                    {
                        string rawValue = instance.CimInstanceProperties["Value"].Value?.ToString() ?? string.Empty;

                        var options = rawValue.Split(',').Select(o => o.Trim()).ToList();
                        string activeValue = options.FirstOrDefault(o => o.StartsWith("*"))?.TrimStart('*') ?? string.Empty;

                        settings.Add(new BiosSettingData
                        {
                            Name = name,
                            CurrentValue = activeValue,
                            IsBinary = options.Any(o => o.EndsWith("Enable", StringComparison.OrdinalIgnoreCase)) &&
                                       options.Any(o => o.EndsWith("Disable", StringComparison.OrdinalIgnoreCase))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to fetch WMI BIOS settings: {ex.Message}");
            }

            return settings;
        }

        public bool SetBiosSetting(string name, string value)
        {
            try
            {
                using var session = CimSession.Create(null);
                var query = "SELECT * FROM HP_BIOSSettingInterface";
                var interfaceInstance = session.QueryInstances(Namespace, "WQL", query).FirstOrDefault();

                if (interfaceInstance != null)
                {
                    var parameters = new CimMethodParametersCollection();
                    parameters.Add(CimMethodParameter.Create("Name", name, CimType.String, CimFlags.None));
                    parameters.Add(CimMethodParameter.Create("Value", value, CimType.String, CimFlags.None));
                    parameters.Add(CimMethodParameter.Create("Password", "<utf-16/>", CimType.String, CimFlags.None));

                    var result = session.InvokeMethod(Namespace, interfaceInstance, "SetBIOSSetting", parameters);
                    var returnVal = result.ReturnValue?.Value?.ToString();

                    return returnVal == "0";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write WMI BIOS setting: {ex.Message}");
            }
            return false;
        }
    }

    public class BiosSettingData
    {
        public string Name { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public bool IsBinary { get; set; }
    }
}