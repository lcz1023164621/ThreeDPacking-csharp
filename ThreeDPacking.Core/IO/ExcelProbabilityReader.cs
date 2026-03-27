using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
                    bool isHighlighted = IsHighlightedRow(sheet, row, 1, 10);

                    // 初始 InstanceId 先占位；后续在 UI 里会重新分配。
                    items.Add(new ItemCandidate(name, dx, dy, dz, 0, probability, isHighlighted));
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

        private static bool IsHighlightedRow(ExcelWorksheet sheet, int row, int startCol, int endCol)
        {
            for (int col = startCol; col <= endCol; col++)
            {
                var fill = sheet.Cells[row, col].Style.Fill;
                if (fill == null || fill.PatternType == ExcelFillStyle.None)
                    continue;

                var color = ParseExcelColor(fill.BackgroundColor);
                if (!color.IsEmpty && IsYellowish(color))
                    return true;

                color = ParseExcelColor(fill.PatternColor);
                if (!color.IsEmpty && IsYellowish(color))
                    return true;
            }

            return false;
        }

        private static Color ParseExcelColor(ExcelColor excelColor)
        {
            if (excelColor == null || string.IsNullOrWhiteSpace(excelColor.Rgb))
                return Color.Empty;

            string rgb = excelColor.Rgb.Trim();
            if (rgb.Length == 8)
                rgb = rgb.Substring(2); // ARGB -> RGB
            if (rgb.Length != 6)
                return Color.Empty;

            if (!int.TryParse(rgb.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int r))
                return Color.Empty;
            if (!int.TryParse(rgb.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int g))
                return Color.Empty;
            if (!int.TryParse(rgb.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int b))
                return Color.Empty;

            return Color.FromArgb(r, g, b);
        }

        private static bool IsYellowish(Color color)
        {
            // 黄色通常 R/G 都高、B 明显偏低；允许少量色偏
            return color.R >= 180 && color.G >= 170 && color.B <= 150;
        }
    }
}

