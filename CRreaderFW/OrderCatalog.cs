using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;

namespace WindowsFormsApp1
{
    internal sealed class OrderCatalog
    {
        private readonly List<OrderCatalogRow> _rows;

        public int Count { get { return _rows.Count; } }

        private OrderCatalog(List<OrderCatalogRow> rows)
        {
            _rows = rows;
        }

        public static OrderCatalog Load(string path)
        {
            if (!File.Exists(path))
            {
                return new OrderCatalog(new List<OrderCatalogRow>());
            }

            List<string[]> table;
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".xls" || extension == ".xlsx")
            {
                table = ReadExcelRows(path);
            }
            else
            {
                table = DelimitedText.Read(path);
            }

            if (table.Count == 0)
            {
                return new OrderCatalog(new List<OrderCatalogRow>());
            }

            string[] headers = table[0].Select(h => (h ?? string.Empty).Trim()).ToArray();
            int orderIndex = FindColumn(headers, "OrderNo", "Order", "订单编号", "订单号");
            int boxIndex = FindColumn(headers, "BoxCode", "Box", "箱码编号", "箱码号", "箱码");
            int barcodeIndex = FindColumn(headers, "Barcode", "BarCode", "条码", "条码号", "条形码号");
            int skuIndex = FindColumn(headers, "Sku", "SKU", "货号", "产品货号");
            int qtyIndex = FindColumn(headers, "OrderQuantity", "Quantity", "Qty", "订单数量", "数量");
            int lengthIndex = FindColumn(headers, "Length", "L", "长", "长度");
            int widthIndex = FindColumn(headers, "Width", "W", "宽", "宽度");
            int heightIndex = FindColumn(headers, "Height", "H", "高", "高度");

            var rows = new List<OrderCatalogRow>();
            for (int i = 1; i < table.Count; i++)
            {
                string[] line = table[i];
                string orderNo = GetCell(line, orderIndex);
                string boxCode = GetCell(line, boxIndex);
                if (orderNo.Length == 0 && boxCode.Length == 0)
                {
                    continue;
                }

                string barcode = ProductCatalog.NormalizeBarcode(GetCell(line, barcodeIndex));
                string sku = GetCell(line, skuIndex);
                int quantity;
                int.TryParse(GetCell(line, qtyIndex), out quantity);
                if (quantity < 0)
                {
                    quantity = 0;
                }

                rows.Add(new OrderCatalogRow(
                    orderNo,
                    boxCode,
                    barcode,
                    sku,
                    quantity,
                    GetCell(line, lengthIndex),
                    GetCell(line, widthIndex),
                    GetCell(line, heightIndex)));
            }

            return new OrderCatalog(rows);
        }

        public OrderLookupResult Lookup(string input, bool byOrderNo)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            string key = input.Trim();
            IEnumerable<OrderCatalogRow> matched = byOrderNo
                ? _rows.Where(r => string.Equals(r.OrderNo, key, StringComparison.OrdinalIgnoreCase))
                : _rows.Where(r => string.Equals(r.BoxCode, key, StringComparison.OrdinalIgnoreCase));

            List<OrderCatalogRow> list = matched.ToList();
            if (list.Count == 0)
            {
                return null;
            }

            string orderNo = list[0].OrderNo;
            string boxCode = list[0].BoxCode;
            if (orderNo.Length == 0 || boxCode.Length == 0)
            {
                return null;
            }

            if (list.Any(r => !string.Equals(r.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(r.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            var items = new List<OrderMatchItem>();
            foreach (OrderCatalogRow row in list)
            {
                if (row.Barcode.Length == 0)
                {
                    continue;
                }

                OrderMatchItem existing = items.FirstOrDefault(i => string.Equals(i.Barcode, row.Barcode, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    items.Add(new OrderMatchItem
                    {
                        Barcode = row.Barcode,
                        Sku = row.Sku,
                        OrderQuantity = row.OrderQuantity,
                        Length = row.Length,
                        Width = row.Width,
                        Height = row.Height
                    });
                }
                else if (row.OrderQuantity > existing.OrderQuantity)
                {
                    existing.OrderQuantity = row.OrderQuantity;
                }
            }

            return new OrderLookupResult(orderNo, boxCode, items);
        }

        private static int FindColumn(string[] headers, params string[] candidates)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim();
                if (candidates.Any(c => string.Equals(c, header, StringComparison.OrdinalIgnoreCase)))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string GetCell(string[] row, int index)
        {
            if (index < 0 || index >= row.Length)
            {
                return string.Empty;
            }

            return (row[index] ?? string.Empty).Trim();
        }

        private static List<string[]> ReadExcelRows(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook workbook = WorkbookFactory.Create(stream);
                ISheet sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                {
                    return new List<string[]>();
                }

                var rows = new List<string[]>();
                var formatter = new DataFormatter();
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
                        string value = cell == null ? string.Empty : formatter.FormatCellValue(cell).Trim();
                        values[cellIndex] = value;
                        hasValue = hasValue || value.Length > 0;
                    }

                    if (hasValue)
                    {
                        rows.Add(values);
                    }
                }

                return rows;
            }
        }

        private sealed class OrderCatalogRow
        {
            public string OrderNo { get; private set; }
            public string BoxCode { get; private set; }
            public string Barcode { get; private set; }
            public string Sku { get; private set; }
            public int OrderQuantity { get; private set; }
            public string Length { get; private set; }
            public string Width { get; private set; }
            public string Height { get; private set; }

            public OrderCatalogRow(string orderNo, string boxCode, string barcode, string sku, int orderQuantity, string length, string width, string height)
            {
                OrderNo = orderNo ?? string.Empty;
                BoxCode = boxCode ?? string.Empty;
                Barcode = barcode ?? string.Empty;
                Sku = sku ?? string.Empty;
                OrderQuantity = orderQuantity;
                Length = length ?? string.Empty;
                Width = width ?? string.Empty;
                Height = height ?? string.Empty;
            }
        }
    }

    internal sealed class OrderLookupResult
    {
        public string OrderNo { get; private set; }
        public string BoxCode { get; private set; }
        public List<OrderMatchItem> Items { get; private set; }

        public OrderLookupResult(string orderNo, string boxCode, List<OrderMatchItem> items)
        {
            OrderNo = orderNo ?? string.Empty;
            BoxCode = boxCode ?? string.Empty;
            Items = items ?? new List<OrderMatchItem>();
        }
    }
}
