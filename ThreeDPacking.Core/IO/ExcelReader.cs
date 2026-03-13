using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// Excel 文件读取，导入物品/容器数据
    /// </summary>
    public static class ExcelReader
    {
        /// <summary>
        /// Read items from the first sheet of an Excel file.
        /// Column 0 = name, columns 5/6/7 = dx/dy/dz (multiplied by 10).
        /// Skips the header row.
        /// </summary>
        public static List<ItemCandidate> ReadItems(string filePath)
        {
            var items = new List<ItemCandidate>();

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("Excel file not found: " + filePath);

            using (var package = new ExcelPackage(fileInfo))
            {
                var sheet = package.Workbook.Worksheets[1]; // EPPlus 4.x: 1-based index
                if (sheet == null)
                    return items;

                int rowCount = sheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                    return items; // Only header or empty

                // Start from row 2 (skip header)
                for (int row = 2; row <= rowCount; row++)
                {
                    string name = GetCellString(sheet, row, 1); // Column A (1-based)
                    string dxText = GetCellString(sheet, row, 6); // Column F
                    string dyText = GetCellString(sheet, row, 7); // Column G
                    string dzText = GetCellString(sheet, row, 8); // Column H

                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(dxText) ||
                        string.IsNullOrWhiteSpace(dyText) ||
                        string.IsNullOrWhiteSpace(dzText))
                        continue;

                    int dx = ParsePositiveInt(dxText) * 10;
                    int dy = ParsePositiveInt(dyText) * 10;
                    int dz = ParsePositiveInt(dzText) * 10;

                    items.Add(new ItemCandidate(name, dx, dy, dz, 0));
                }
            }

            return items;
        }

        private static string GetCellString(ExcelWorksheet sheet, int row, int col)
        {
            var cell = sheet.Cells[row, col];
            if (cell?.Value == null) return "";
            return cell.Value.ToString().Trim();
        }

        private static int ParsePositiveInt(string text)
        {
            double value = double.Parse(text);
            int result = (int)Math.Round(value);
            if (result <= 0)
                throw new ArgumentException("Dimension must be positive, got: " + text);
            return result;
        }
    }
}
