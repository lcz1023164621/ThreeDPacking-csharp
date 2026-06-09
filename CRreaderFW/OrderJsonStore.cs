using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace WindowsFormsApp1
{
    [DataContract]
    internal sealed class OrderJsonDocument
    {
        [DataMember(Name = "orderNo")]
        public string OrderNo { get; set; }

        [DataMember(Name = "boxCode")]
        public string BoxCode { get; set; }

        [DataMember(Name = "items")]
        public List<OrderJsonItem> Items { get; set; }
    }

    [DataContract]
    internal sealed class OrderJsonItem
    {
        [DataMember(Name = "sku")]
        public string Sku { get; set; }

        [DataMember(Name = "barcode")]
        public string Barcode { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }

        [DataMember(Name = "length")]
        public string Length { get; set; }

        [DataMember(Name = "width")]
        public string Width { get; set; }

        [DataMember(Name = "height")]
        public string Height { get; set; }
    }

    internal static class OrderJsonStore
    {
        public static string GetOrdersDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "orders");
        }

        public static string BuildJsonPath(string orderNo, string boxCode)
        {
            string safeOrder = SanitizeFileName(orderNo);
            string safeBox = SanitizeFileName(boxCode);
            return Path.Combine(GetOrdersDirectory(), safeOrder + "-" + safeBox + ".json");
        }

        public static OrderJsonDocument FromLookupResult(OrderLookupResult lookup)
        {
            if (lookup == null)
            {
                return null;
            }

            var document = new OrderJsonDocument
            {
                OrderNo = lookup.OrderNo,
                BoxCode = lookup.BoxCode,
                Items = new List<OrderJsonItem>()
            };

            foreach (OrderMatchItem item in lookup.Items)
            {
                document.Items.Add(new OrderJsonItem
                {
                    Sku = item.Sku ?? string.Empty,
                    Barcode = item.Barcode ?? string.Empty,
                    Quantity = item.OrderQuantity,
                    Length = item.Length ?? string.Empty,
                    Width = item.Width ?? string.Empty,
                    Height = item.Height ?? string.Empty
                });
            }

            return document;
        }

        public static OrderLookupResult ToLookupResult(OrderJsonDocument document)
        {
            if (document == null ||
                string.IsNullOrWhiteSpace(document.OrderNo) ||
                string.IsNullOrWhiteSpace(document.BoxCode))
            {
                return null;
            }

            var items = new List<OrderMatchItem>();
            if (document.Items != null)
            {
                foreach (OrderJsonItem item in document.Items)
                {
                    if (item == null || string.IsNullOrWhiteSpace(item.Barcode))
                    {
                        continue;
                    }

                    items.Add(new OrderMatchItem
                    {
                        Barcode = ProductCatalog.NormalizeBarcode(item.Barcode),
                        Sku = item.Sku ?? string.Empty,
                        OrderQuantity = item.Quantity < 0 ? 0 : item.Quantity,
                        Length = item.Length ?? string.Empty,
                        Width = item.Width ?? string.Empty,
                        Height = item.Height ?? string.Empty
                    });
                }
            }

            return new OrderLookupResult(document.OrderNo.Trim(), document.BoxCode.Trim(), items);
        }

        public static void Save(OrderJsonDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            string path = BuildJsonPath(document.OrderNo, document.BoxCode);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? GetOrdersDirectory());

            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(OrderJsonDocument));
                serializer.WriteObject(stream, document);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(path, FormatJson(json), Encoding.UTF8);
            }
        }

        public static OrderJsonDocument Load(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (var stream = File.OpenRead(path))
            {
                var serializer = new DataContractJsonSerializer(typeof(OrderJsonDocument));
                return serializer.ReadObject(stream) as OrderJsonDocument;
            }
        }

        private static string FormatJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return "{}";
            }

            return json;
        }

        private static string SanitizeFileName(string value)
        {
            string safe = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(ch, '_');
            }

            return safe;
        }
    }
}
