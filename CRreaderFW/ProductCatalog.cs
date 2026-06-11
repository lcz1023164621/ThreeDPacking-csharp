using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;

namespace WindowsFormsApp1
{
    internal sealed class ProductCatalog
    {
        private readonly Dictionary<string, ProductRecord> _byBarcode;

        public IReadOnlyList<string> Headers { get; private set; }
        public int Count { get { return _byBarcode.Count; } }
        public IReadOnlyCollection<ProductRecord> Records { get { return _byBarcode.Values.ToList(); } }

        private ProductCatalog(IReadOnlyList<string> headers, Dictionary<string, ProductRecord> byBarcode)
        {
            Headers = headers;
            _byBarcode = byBarcode;
        }

        public static ProductCatalog Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("ProductInfo file not found.", path);
            }

            List<string[]> rows;
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".xls" || extension == ".xlsx")
            {
                rows = ReadExcelRows(path);
            }
            else
            {
                rows = DelimitedText.Read(path);
            }

            if (rows.Count == 0)
            {
                throw new InvalidOperationException("ProductInfo is empty: " + path);
            }

            string[] headers = rows[0].Select(h => (h ?? string.Empty).Trim()).ToArray();
            int barcodeIndex = FindBarcodeColumn(headers);
            var map = new Dictionary<string, ProductRecord>(StringComparer.OrdinalIgnoreCase);

            for (int i = 1; i < rows.Count; i++)
            {
                string[] row = rows[i];
                if (barcodeIndex >= row.Length)
                {
                    continue;
                }

                string barcode = NormalizeBarcode(row[barcodeIndex]);
                if (barcode.Length == 0)
                {
                    continue;
                }

                var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int col = 0; col < headers.Length; col++)
                {
                    string header = headers[col];
                    if (header.Length == 0)
                    {
                        header = "Column" + (col + 1);
                    }

                    values[header] = col < row.Length ? row[col] : string.Empty;
                }

                map[barcode] = new ProductRecord(barcode, values);
            }

            return new ProductCatalog(headers, map);
        }

        public ProductRecord Find(string barcode)
        {
            ProductRecord record;
            string normalized = NormalizeBarcode(barcode);
            return _byBarcode.TryGetValue(normalized, out record) ? record : null;
        }

        public ProductRecord FindBySku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                return null;
            }

            string key = sku.Trim();
            foreach (ProductRecord record in _byBarcode.Values)
            {
                string candidate = record.GetValue("SKU", "Sku", "sku", "货号", "产品货号");
                if (string.Equals(candidate, key, StringComparison.OrdinalIgnoreCase))
                {
                    return record;
                }
            }

            return null;
        }

        public static string NormalizeBarcode(string barcode)
        {
            if (barcode == null)
            {
                return string.Empty;
            }

            var chars = new List<char>();
            foreach (char ch in barcode.Trim())
            {
                if (!char.IsWhiteSpace(ch) && !char.IsControl(ch))
                {
                    chars.Add(ch);
                }
            }

            return new string(chars.ToArray());
        }

        private static int FindBarcodeColumn(string[] headers)
        {
            string[] candidates =
            {
                "Barcode",
                "BarCode",
                "Code",
                "条码",
                "条码号",
                "条码数字",
                "物料条码",
                "产品条码"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim();
                if (candidates.Any(c => string.Equals(c, header, StringComparison.OrdinalIgnoreCase)))
                {
                    return i;
                }
            }

            return 0;
        }

        private static List<string[]> ReadExcelRows(string path)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    IWorkbook workbook = WorkbookFactory.Create(stream);
                    ISheet sheet = workbook.GetSheetAt(0);
                    if (sheet == null)
                    {
                        throw new InvalidOperationException("No sheet found in ProductInfo: " + path);
                    }

                    var rows = new List<string[]>();
                    var formatter = new DataFormatter();
                    int barcodeColumnIndex = -1;
                    for (int rowIndex = sheet.FirstRowNum; rowIndex <= sheet.LastRowNum; rowIndex++)
                    {
                        IRow row = sheet.GetRow(rowIndex);
                        if (row == null)
                        {
                            continue;
                        }

                        int lastCell = row.LastCellNum;
                        if (lastCell < 0)
                        {
                            continue;
                        }

                        string[] values = new string[lastCell];
                        bool hasValue = false;
                        for (int cellIndex = 0; cellIndex < lastCell; cellIndex++)
                        {
                            ICell cell = row.GetCell(cellIndex);
                            bool barcodeColumn = cellIndex == barcodeColumnIndex;
                            string value = SpreadsheetCellReader.ReadCell(cell, formatter, barcodeColumn);
                            values[cellIndex] = value;
                            hasValue = hasValue || value.Length > 0;
                        }

                        if (hasValue)
                        {
                            if (barcodeColumnIndex < 0)
                            {
                                barcodeColumnIndex = FindBarcodeColumn(values);
                            }
                            rows.Add(values);
                        }
                    }

                    return rows;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot read Excel ProductInfo. Please check the file is a valid .xls/.xlsx workbook. " + ex.Message, ex);
            }
        }
    }

    internal sealed class ProductRecord
    {
        public string Barcode { get; private set; }
        public IReadOnlyDictionary<string, string> Values { get; private set; }

        public ProductRecord(string barcode, IReadOnlyDictionary<string, string> values)
        {
            Barcode = barcode;
            Values = values;
        }

        public string GetValue(params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                foreach (string header in Values.Keys)
                {
                    if (string.Equals(header, candidate, StringComparison.OrdinalIgnoreCase) ||
                        header.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return Values[header];
                    }
                }
            }

            return string.Empty;
        }
    }
}
