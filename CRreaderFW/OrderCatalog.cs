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
            _rows = rows ?? new List<OrderCatalogRow>();
        }

        public static OrderCatalog Empty()
        {
            return new OrderCatalog(new List<OrderCatalogRow>());
        }

        public static OrderCatalog Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return Empty();
            }

            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".json")
            {
                return LoadJson(path);
            }

            ParseOrderFileName(Path.GetFileNameWithoutExtension(path), out string orderNoFromFile, out string boxCodeFromFile);
            List<string[]> table = ReadTable(path);
            return new OrderCatalog(ParseRows(table, orderNoFromFile, boxCodeFromFile));
        }

        public static OrderCatalog LoadFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                return Empty();
            }

            var rows = new List<OrderCatalogRow>();
            foreach (string path in Directory.EnumerateFiles(folderPath))
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension != ".csv" && extension != ".txt" && extension != ".tsv" &&
                    extension != ".xls" && extension != ".xlsx" && extension != ".json")
                {
                    continue;
                }

                OrderCatalog catalog = Load(path);
                rows.AddRange(catalog._rows);
            }

            return new OrderCatalog(rows);
        }

        public static OrderCatalog LoadOrdersDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                return Empty();
            }

            var rows = new List<OrderCatalogRow>();
            foreach (string path in Directory.EnumerateFiles(directoryPath, "*.json"))
            {
                OrderCatalog catalog = LoadJson(path);
                rows.AddRange(catalog._rows);
            }

            return new OrderCatalog(rows);
        }

        public static OrderCatalog Combine(params OrderCatalog[] catalogs)
        {
            var rows = new List<OrderCatalogRow>();
            if (catalogs == null)
            {
                return Empty();
            }

            foreach (OrderCatalog catalog in catalogs)
            {
                if (catalog == null || catalog._rows.Count == 0)
                {
                    continue;
                }

                rows.AddRange(catalog._rows);
            }

            return new OrderCatalog(rows);
        }

        public IReadOnlyList<OrderBoxPair> GetDistinctBindings()
        {
            return _rows
                .Where(r => r.OrderNo.Length > 0 && r.BoxCode.Length > 0)
                .GroupBy(r => r.OrderNo + "\u001f" + r.BoxCode, StringComparer.OrdinalIgnoreCase)
                .Select(g =>
                {
                    OrderCatalogRow first = g.First();
                    return new OrderBoxPair(first.OrderNo, first.BoxCode);
                })
                .ToList();
        }

        public static IReadOnlyList<OrderCatalogConflict> FindConflicts(OrderCatalog existing, OrderCatalog incoming)
        {
            var conflicts = new List<OrderCatalogConflict>();
            if (existing == null || incoming == null)
            {
                return conflicts;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (OrderBoxPair incomingPair in incoming.GetDistinctBindings())
            {
                OrderBoxPair orderConflict = existing.FindBindingByOrderNo(incomingPair.OrderNo);
                if (orderConflict != null && !orderConflict.EqualsPair(incomingPair))
                {
                    string key = "O|" + incomingPair.OrderNo + "|" + orderConflict.BoxCode + "|" + incomingPair.BoxCode;
                    if (seen.Add(key))
                    {
                        conflicts.Add(new OrderCatalogConflict("订单号", orderConflict, incomingPair));
                    }
                }

                OrderBoxPair boxConflict = existing.FindBindingByBoxCode(incomingPair.BoxCode);
                if (boxConflict != null && !boxConflict.EqualsPair(incomingPair))
                {
                    string key = "B|" + incomingPair.BoxCode + "|" + boxConflict.OrderNo + "|" + incomingPair.OrderNo;
                    if (seen.Add(key))
                    {
                        conflicts.Add(new OrderCatalogConflict("箱码号", boxConflict, incomingPair));
                    }
                }
            }

            return conflicts;
        }

        public static OrderCatalog Merge(OrderCatalog existing, OrderCatalog incoming, bool overwriteConflicts)
        {
            if (incoming == null || incoming._rows.Count == 0)
            {
                return existing ?? Empty();
            }

            if (existing == null || existing._rows.Count == 0)
            {
                return incoming;
            }

            IReadOnlyList<OrderCatalogConflict> conflicts = FindConflicts(existing, incoming);
            var conflictIncomingPairs = new HashSet<string>(
                conflicts.Select(c => c.Incoming.Key),
                StringComparer.OrdinalIgnoreCase);

            OrderCatalog result = existing;
            foreach (OrderBoxPair incomingPair in incoming.GetDistinctBindings())
            {
                bool hasConflict = conflictIncomingPairs.Contains(incomingPair.Key);
                if (hasConflict && !overwriteConflicts)
                {
                    continue;
                }

                result = result.WithoutOrderOrBox(incomingPair.OrderNo, incomingPair.BoxCode);
                result = result.WithRows(incoming.GetRowsForBinding(incomingPair));
            }

            return result;
        }

        private OrderBoxPair FindBindingByOrderNo(string orderNo)
        {
            OrderCatalogRow row = _rows.FirstOrDefault(r =>
                string.Equals(r.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase));
            return row == null ? null : new OrderBoxPair(row.OrderNo, row.BoxCode);
        }

        private OrderBoxPair FindBindingByBoxCode(string boxCode)
        {
            OrderCatalogRow row = _rows.FirstOrDefault(r =>
                string.Equals(r.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase));
            return row == null ? null : new OrderBoxPair(row.OrderNo, row.BoxCode);
        }

        private IEnumerable<OrderCatalogRow> GetRowsForBinding(OrderBoxPair pair)
        {
            return _rows.Where(r =>
                string.Equals(r.OrderNo, pair.OrderNo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.BoxCode, pair.BoxCode, StringComparison.OrdinalIgnoreCase));
        }

        private OrderCatalog WithoutOrderOrBox(string orderNo, string boxCode)
        {
            return new OrderCatalog(_rows.Where(r =>
                !string.Equals(r.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(r.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        private OrderCatalog WithRows(IEnumerable<OrderCatalogRow> rows)
        {
            var merged = new List<OrderCatalogRow>(_rows);
            merged.AddRange(rows);
            return new OrderCatalog(merged);
        }

        /// <summary>
        /// 只读加载订单文件或文件夹，不修改源文件，也不写入其他位置。
        /// </summary>
        public static OrderCatalog LoadPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("路径不能为空。", nameof(path));
            }

            return Directory.Exists(path) ? LoadFolder(path) : Load(path);
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

            return new OrderLookupResult(orderNo, boxCode, BuildMatchItems(list));
        }

        public static bool TryParseOrderFileName(string fileNameWithoutExtension, out string orderNo, out string boxCode)
        {
            orderNo = string.Empty;
            boxCode = string.Empty;
            if (string.IsNullOrWhiteSpace(fileNameWithoutExtension))
            {
                return false;
            }

            string name = fileNameWithoutExtension.Trim();
            int dashIndex = name.IndexOf('-');
            if (dashIndex <= 0 || dashIndex >= name.Length - 1)
            {
                return false;
            }

            orderNo = name.Substring(0, dashIndex).Trim();
            boxCode = name.Substring(dashIndex + 1).Trim();
            return orderNo.Length > 0 && boxCode.Length > 0;
        }

        private static void ParseOrderFileName(string fileNameWithoutExtension, out string orderNo, out string boxCode)
        {
            TryParseOrderFileName(fileNameWithoutExtension, out orderNo, out boxCode);
        }

        private static OrderCatalog LoadJson(string path)
        {
            OrderJsonDocument document = OrderJsonStore.Load(path);
            OrderLookupResult lookup = OrderJsonStore.ToLookupResult(document);
            if (lookup == null)
            {
                return Empty();
            }

            var rows = new List<OrderCatalogRow>();
            foreach (OrderMatchItem item in lookup.Items)
            {
                rows.Add(new OrderCatalogRow(
                    lookup.OrderNo,
                    lookup.BoxCode,
                    item.Barcode,
                    item.Sku,
                    item.OrderQuantity,
                    item.Length,
                    item.Width,
                    item.Height));
            }

            return new OrderCatalog(rows);
        }

        private static List<string[]> ReadTable(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".xls" || extension == ".xlsx")
            {
                return ReadExcelRows(path);
            }

            return DelimitedText.Read(path);
        }

        private static List<OrderCatalogRow> ParseRows(List<string[]> table, string orderNoFromFile, string boxCodeFromFile)
        {
            if (table.Count == 0)
            {
                return new List<OrderCatalogRow>();
            }

            string[] headers = table[0].Select(h => (h ?? string.Empty).Trim()).ToArray();
            int orderIndex = FindColumn(headers, "OrderNo", "Order", "订单编号", "订单号");
            int boxIndex = FindColumn(headers, "BoxCode", "Box", "箱码编号", "箱码号", "箱码");
            int barcodeIndex = FindColumn(headers, "Barcode", "BarCode", "条码", "条码号", "条形码号");
            int skuIndex = FindColumn(headers, "Sku", "SKU", "货号", "产品货号");
            int qtyIndex = FindColumn(headers, "OrderQuantity", "Quantity", "Qty", "订单数量", "数量");
            int lengthIndex = FindColumn(headers, "Length_mm", "Length", "L", "长", "长度");
            int widthIndex = FindColumn(headers, "Width_mm", "Width", "W", "宽", "宽度");
            int heightIndex = FindColumn(headers, "Height_mm", "Height", "H", "高", "高度");

            var rows = new List<OrderCatalogRow>();
            for (int i = 1; i < table.Count; i++)
            {
                string[] line = table[i];
                string orderNo = GetCell(line, orderIndex);
                string boxCode = GetCell(line, boxIndex);
                if (orderNo.Length == 0)
                {
                    orderNo = orderNoFromFile;
                }
                if (boxCode.Length == 0)
                {
                    boxCode = boxCodeFromFile;
                }

                string sku = GetCell(line, skuIndex);
                string barcode = NormalizeOrderBarcode(GetCell(line, barcodeIndex));
                if (barcode.Length == 0 && sku.Length == 0)
                {
                    continue;
                }

                if (orderNo.Length == 0 || boxCode.Length == 0)
                {
                    continue;
                }

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

            return rows;
        }

        private static List<OrderMatchItem> BuildMatchItems(IEnumerable<OrderCatalogRow> rows)
        {
            var items = new List<OrderMatchItem>();
            foreach (OrderCatalogRow row in rows)
            {
                if (row.Barcode.Length == 0 && row.Sku.Length == 0)
                {
                    continue;
                }

                OrderMatchItem existing = items.FirstOrDefault(i => IsSameOrderLine(i, row));
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
                else
                {
                    existing.OrderQuantity += row.OrderQuantity;
                    if (string.IsNullOrWhiteSpace(existing.Length) && row.Length.Length > 0)
                    {
                        existing.Length = row.Length;
                    }
                    if (string.IsNullOrWhiteSpace(existing.Width) && row.Width.Length > 0)
                    {
                        existing.Width = row.Width;
                    }
                    if (string.IsNullOrWhiteSpace(existing.Height) && row.Height.Length > 0)
                    {
                        existing.Height = row.Height;
                    }
                }
            }

            return items;
        }

        private static bool IsSameOrderLine(OrderMatchItem item, OrderCatalogRow row)
        {
            if (item == null || row == null)
            {
                return false;
            }

            if (item.Sku.Length > 0 && row.Sku.Length > 0)
            {
                return string.Equals(item.Sku, row.Sku, StringComparison.OrdinalIgnoreCase);
            }

            return item.Barcode.Length > 0 &&
                row.Barcode.Length > 0 &&
                string.Equals(item.Barcode, row.Barcode, StringComparison.OrdinalIgnoreCase);
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

        private static string NormalizeOrderBarcode(string raw)
        {
            return SpreadsheetCellReader.NormalizeBarcodeText(raw);
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
                            string[] headers = values.Select(v => (v ?? string.Empty).Trim()).ToArray();
                            barcodeColumnIndex = FindColumn(headers, "Barcode", "BarCode", "条码", "条码号", "条形码号");
                        }

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

    internal sealed class OrderBoxPair
    {
        public string OrderNo { get; private set; }
        public string BoxCode { get; private set; }

        public string Key
        {
            get { return OrderNo + "\u001f" + BoxCode; }
        }

        public OrderBoxPair(string orderNo, string boxCode)
        {
            OrderNo = orderNo ?? string.Empty;
            BoxCode = boxCode ?? string.Empty;
        }

        public bool EqualsPair(OrderBoxPair other)
        {
            if (other == null)
            {
                return false;
            }

            return string.Equals(OrderNo, other.OrderNo, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(BoxCode, other.BoxCode, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed class OrderCatalogConflict
    {
        public string ConflictField { get; private set; }
        public OrderBoxPair Existing { get; private set; }
        public OrderBoxPair Incoming { get; private set; }

        public OrderCatalogConflict(string conflictField, OrderBoxPair existing, OrderBoxPair incoming)
        {
            ConflictField = conflictField ?? string.Empty;
            Existing = existing;
            Incoming = incoming;
        }

        public string Describe()
        {
            if (string.Equals(ConflictField, "订单号", StringComparison.OrdinalIgnoreCase))
            {
                return string.Format(
                    "订单号 {0}：已有箱码 {1}，新记录箱码 {2}",
                    Existing.OrderNo,
                    Existing.BoxCode,
                    Incoming.BoxCode);
            }

            return string.Format(
                "箱码 {0}：已有订单 {1}，新记录订单 {2}",
                Existing.BoxCode,
                Existing.OrderNo,
                Incoming.OrderNo);
        }
    }
}
