using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1
{
    internal sealed class BatchStatisticsWriter
    {
        private readonly string _outputDirectory;

        public BatchStatisticsWriter(string outputDirectory)
        {
            _outputDirectory = outputDirectory;
        }

        public string WriteBatch(string fileStem, string orderNo, string boxCode, IEnumerable<ScanRecord> records, ProductCatalog catalog)
        {
            Directory.CreateDirectory(_outputDirectory);
            string safeStem = SanitizeFileName(string.IsNullOrWhiteSpace(fileStem) ? "batch" : fileStem);
            string path = Path.Combine(_outputDirectory, safeStem + ".csv");

            var rows = records
                .GroupBy(r => r.Barcode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            using (var writer = new StreamWriter(path, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(DelimitedText.ToCsvLine(new[]
                {
                    "订单编号",
                    "箱码编号",
                    "条码号",
                    "货号",
                    "长",
                    "宽",
                    "高",
                    "数量",
                    "状态"
                }));

                foreach (var group in rows)
                {
                    if (string.IsNullOrWhiteSpace(group.Key))
                    {
                        continue;
                    }

                    ScanRecord first = group.First();
                    ProductRecord product = catalog == null ? null : catalog.Find(group.Key);
                    string sku = FirstNonEmpty(first.Sku, GetProductSku(product));
                    string length = FirstNonEmpty(first.Length, GetProductLength(product));
                    string width = FirstNonEmpty(first.Width, GetProductWidth(product));
                    string height = FirstNonEmpty(first.Height, GetProductHeight(product));

                    writer.WriteLine(DelimitedText.ToCsvLine(new[]
                    {
                        orderNo ?? string.Empty,
                        boxCode ?? string.Empty,
                        group.Key,
                        sku,
                        length,
                        width,
                        height,
                        group.Count().ToString(),
                        first.Status ?? string.Empty
                    }));
                }
            }

            return path;
        }

        public void DeleteBatchIfExists(string fileStem)
        {
            if (string.IsNullOrWhiteSpace(fileStem))
            {
                return;
            }

            string path = Path.Combine(_outputDirectory, SanitizeFileName(fileStem) + ".csv");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static string GetProductSku(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("SKU", "Sku", "sku", "货号");
        }

        private static string GetProductLength(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Length", "L", "长", "长度");
        }

        private static string GetProductWidth(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Width", "W", "宽", "宽度");
        }

        private static string GetProductHeight(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Height", "H", "高", "高度");
        }

        private static string FirstNonEmpty(string primary, string fallback)
        {
            return string.IsNullOrWhiteSpace(primary) ? (fallback ?? string.Empty) : primary;
        }

        private static string SanitizeFileName(string value)
        {
            string safe = value.Trim();
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(ch, '_');
            }
            return safe;
        }
    }
}
