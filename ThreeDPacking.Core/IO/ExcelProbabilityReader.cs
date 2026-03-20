using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using OfficeOpenXml;

namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// Excel 文件读取（带物品出现概率）。
    /// 约定：列 A=名称(1)，列 F/G/H=Dx/Dy/Dz(6/7/8)，列 J=出现概率(10)。
    /// </summary>
    public static class ExcelProbabilityReader
    {
        public static List<ItemCandidate> ReadItems(string filePath)
        {
            var items = new List<ItemCandidate>();

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("Excel file not found: " + filePath);

            using (var package = new ExcelPackage(fileInfo))
            {
                var sheet = package.Workbook.Worksheets[1]; // 1-based index
                if (sheet == null)
                    return items;

                int rowCount = sheet.Dimension?.Rows ?? 0;
                if (rowCount <= 1)
                    return items; // Only header or empty

                // Start from row 2 (skip header)
                for (int row = 2; row <= rowCount; row++)
                {
                    string name = GetCellString(sheet, row, 1); // Column A
                    string dxText = GetCellString(sheet, row, 6); // Column F
                    string dyText = GetCellString(sheet, row, 7); // Column G
                    string dzText = GetCellString(sheet, row, 8); // Column H
                    string probText = GetCellString(sheet, row, 10); // Column J

                    if (string.IsNullOrWhiteSpace(name) ||
                        string.IsNullOrWhiteSpace(dxText) ||
                        string.IsNullOrWhiteSpace(dyText) ||
                        string.IsNullOrWhiteSpace(dzText))
                    {
                        continue;
                    }

                    int dx = ParsePositiveInt(dxText) * 10;
                    int dy = ParsePositiveInt(dyText) * 10;
                    int dz = ParsePositiveInt(dzText) * 10;

                    double probability = ParseNonNegativeDouble(probText);

                    // 初始 InstanceId 先占位；后续在 UI 里会重新分配。
                    items.Add(new ItemCandidate(name, dx, dy, dz, 0, probability));
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
            if (!TryParseDouble(text, out var value))
                throw new ArgumentException("Dimension must be a number, got: " + text);
            int result = (int)Math.Round(value);
            if (result <= 0)
                throw new ArgumentException("Dimension must be positive, got: " + text);
            return result;
        }

        private static double ParseNonNegativeDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return 0;

            text = text.Trim();
            if (text.EndsWith("%", StringComparison.Ordinal))
            {
                var numberPart = text.Substring(0, text.Length - 1).Trim();
                if (!TryParseDouble(numberPart, out var percent))
                    return 0;
                return Math.Max(0, percent / 100.0);
            }

            if (!TryParseDouble(text, out var value))
                return 0;

            return Math.Max(0, value);
        }

        private static bool TryParseDouble(string text, out double value)
        {
            // 尝试：Invariant（'.'小数点）、Current（可能使用','）、以及替换小数点。
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                return true;

            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
                return true;

            // 某些 Excel 导出可能混用小数点字符
            if (text.IndexOf(',') >= 0)
            {
                var replaced = text.Replace(',', '.');
                return double.TryParse(replaced, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            }

            value = 0;
            return false;
        }
    }
}

