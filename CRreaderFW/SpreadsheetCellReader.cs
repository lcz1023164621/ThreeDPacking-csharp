using System;
using System.Globalization;
using NPOI.SS.UserModel;

namespace WindowsFormsApp1
{
    internal static class SpreadsheetCellReader
    {
        public static string ReadCell(ICell cell, DataFormatter formatter, bool barcodeColumn)
        {
            if (cell == null)
            {
                return string.Empty;
            }

            if (barcodeColumn)
            {
                string barcode = TryReadBarcodeNumeric(cell);
                if (barcode.Length > 0)
                {
                    return barcode;
                }
            }

            return (formatter == null ? string.Empty : formatter.FormatCellValue(cell) ?? string.Empty).Trim();
        }

        public static string NormalizeBarcodeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            text = text.Trim();
            if (text.ToUpperInvariant().IndexOf('E') >= 0 &&
                double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double scientific))
            {
                return FormatBarcodeNumber(scientific);
            }

            return ProductCatalog.NormalizeBarcode(text);
        }

        private static string TryReadBarcodeNumeric(ICell cell)
        {
            try
            {
                switch (cell.CellType)
                {
                    case CellType.Numeric:
                        return FormatBarcodeNumber(cell.NumericCellValue);
                    case CellType.String:
                        return NormalizeBarcodeText(cell.StringCellValue);
                    case CellType.Formula:
                        switch (cell.CachedFormulaResultType)
                        {
                            case CellType.Numeric:
                                return FormatBarcodeNumber(cell.NumericCellValue);
                            case CellType.String:
                                return NormalizeBarcodeText(cell.StringCellValue);
                        }
                        break;
                }
            }
            catch
            {
                // fall back to default formatting
            }

            return string.Empty;
        }

        private static string FormatBarcodeNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
            {
                return string.Empty;
            }

            return ((long)Math.Round(value, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture);
        }
    }
}
