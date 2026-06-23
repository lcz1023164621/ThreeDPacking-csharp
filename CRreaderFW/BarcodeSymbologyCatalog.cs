using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1
{
    internal enum BarcodeSymbologyCategory
    {
        OneDimensional,
        TwoDimensional,
        Stacked
    }

    internal sealed class BarcodeSymbologyEntry
    {
        public string SdkKey { get; set; }
        public string DisplayName { get; set; }
        public BarcodeSymbologyCategory Category { get; set; }
    }

    internal static class BarcodeSymbologyCatalog
    {
        private static readonly BarcodeSymbologyEntry[] Entries =
        {
            new BarcodeSymbologyEntry { SdkKey = "CODE39", DisplayName = "Code 39", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "CODE128", DisplayName = "Code 128", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "CODABAR", DisplayName = "CodaBar", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "ITF25", DisplayName = "ITF25", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "CODE93", DisplayName = "Code 93", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "MATRIX25", DisplayName = "Matrix 25", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "MSI", DisplayName = "MSI", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "CODE11", DisplayName = "Code 11", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "INDUSTRIAL25", DisplayName = "Industrial 2of5", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "CHINAPOST", DisplayName = "China Post", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "ITF14", DisplayName = "ITF14", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "EAN8", DisplayName = "EAN8", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "EAN13", DisplayName = "EAN13", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "UPCA", DisplayName = "UPCA", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "UPCE", DisplayName = "UPCE", Category = BarcodeSymbologyCategory.OneDimensional },
            new BarcodeSymbologyEntry { SdkKey = "QRCode", DisplayName = "QR Code", Category = BarcodeSymbologyCategory.TwoDimensional },
            new BarcodeSymbologyEntry { SdkKey = "DMCode", DisplayName = "DataMatrix", Category = BarcodeSymbologyCategory.TwoDimensional },
            new BarcodeSymbologyEntry { SdkKey = "PDF417", DisplayName = "PDF417", Category = BarcodeSymbologyCategory.Stacked }
        };

        public static IReadOnlyList<BarcodeSymbologyEntry> All
        {
            get { return Entries; }
        }

        public static HashSet<string> CreateDefaultEnabledSet()
        {
            return new HashSet<string>(Entries.Select(entry => entry.SdkKey));
        }

        public static string FormatEnabledSet(HashSet<string> enabledKeys)
        {
            if (enabledKeys == null || enabledKeys.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(",", Entries
                .Where(entry => enabledKeys.Contains(entry.SdkKey))
                .Select(entry => entry.SdkKey));
        }

        public static HashSet<string> ParseEnabledSet(string value)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }

            foreach (string part in value.Split(','))
            {
                string key = part.Trim();
                if (key.Length == 0)
                {
                    continue;
                }

                if (Entries.Any(entry => string.Equals(entry.SdkKey, key, StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(Entries.First(entry => string.Equals(entry.SdkKey, key, StringComparison.OrdinalIgnoreCase)).SdkKey);
                }
            }

            return result;
        }

        public static HashSet<string> CloneEnabledSet(HashSet<string> enabledKeys)
        {
            if (enabledKeys == null || enabledKeys.Count == 0)
            {
                return CreateDefaultEnabledSet();
            }

            return new HashSet<string>(enabledKeys);
        }
    }
}
