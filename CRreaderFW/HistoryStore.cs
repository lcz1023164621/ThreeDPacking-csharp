using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WindowsFormsApp1
{
    internal sealed class HistoryStore
    {
        private readonly string _rootDirectory;
        private readonly string _recordsPath;

        public HistoryStore(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            _recordsPath = Path.Combine(_rootDirectory, "scan_records.csv");
        }

        public string ImageDirectory
        {
            get { return Path.Combine(_rootDirectory, "images"); }
        }

        public void EnsureCreated()
        {
            Directory.CreateDirectory(_rootDirectory);
            Directory.CreateDirectory(ImageDirectory);
            if (!File.Exists(_recordsPath))
            {
                File.WriteAllText(_recordsPath, "Mode,OrderNo,BatchBoxCode,Sequence,Barcode,Sku,Length,Width,Height,Status,ScanCount,ScanTime,ImagePath" + Environment.NewLine, new UTF8Encoding(true));
            }
        }

        public void Append(ScanRecord record)
        {
            EnsureCreated();
            using (var writer = new StreamWriter(_recordsPath, true, new UTF8Encoding(true)))
            {
                writer.WriteLine(ToCsvLine(new[]
                {
                    record.Mode,
                    record.OrderNo,
                    record.BatchBoxCode,
                    record.Sequence.ToString(),
                    record.Barcode,
                    record.Sku,
                    record.Length,
                    record.Width,
                    record.Height,
                    record.Status,
                    record.ScanCount.ToString(),
                    record.ScanTime.ToString("o"),
                    record.ImagePath
                }));
            }
        }

        public List<ScanRecord> LoadAll()
        {
            EnsureCreated();
            var records = new List<ScanRecord>();
            bool first = true;
            foreach (string line in File.ReadLines(_recordsPath, Encoding.UTF8))
            {
                if (first)
                {
                    first = false;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] values = ParseCsvLine(line).ToArray();
                if (values.Length < 10)
                {
                    continue;
                }

                int sequence;
                int scanCount;
                DateTime scanTime;
                int.TryParse(values[3], out sequence);

                bool hasDimensions = values.Length >= 13;
                string length = hasDimensions ? values[6] : string.Empty;
                string width = hasDimensions ? values[7] : string.Empty;
                string height = hasDimensions ? values[8] : string.Empty;
                string status = hasDimensions ? values[9] : values[6];
                string scanCountText = hasDimensions ? values[10] : values[7];
                string scanTimeText = hasDimensions ? values[11] : values[8];
                string imagePath = hasDimensions ? values[12] : values[9];

                int.TryParse(scanCountText, out scanCount);
                if (!DateTime.TryParse(scanTimeText, out scanTime))
                {
                    scanTime = DateTime.MinValue;
                }

                records.Add(new ScanRecord
                {
                    Mode = values[0],
                    OrderNo = values[1],
                    BatchBoxCode = values[2],
                    Sequence = sequence,
                    Barcode = values[4],
                    Sku = values[5],
                    Length = length,
                    Width = width,
                    Height = height,
                    Status = status,
                    ScanCount = scanCount,
                    ScanTime = scanTime,
                    ImagePath = imagePath
                });
            }

            return records;
        }

        public List<ScanRecord> Search(string orderNo, string batchBoxCode)
        {
            return LoadAll()
                .Where(r =>
                    (!string.IsNullOrWhiteSpace(orderNo) && string.Equals(r.OrderNo, orderNo.Trim(), StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(batchBoxCode) && string.Equals(r.BatchBoxCode, batchBoxCode.Trim(), StringComparison.OrdinalIgnoreCase)))
                .OrderBy(r => r.ScanTime)
                .ThenBy(r => r.Sequence)
                .ToList();
        }

        private static string ToCsvLine(IEnumerable<string> values)
        {
            var parts = new List<string>();
            foreach (string value in values)
            {
                string safe = value ?? string.Empty;
                bool mustQuote = safe.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
                safe = safe.Replace("\"", "\"\"");
                parts.Add(mustQuote ? "\"" + safe + "\"" : safe);
            }
            return string.Join(",", parts);
        }

        private static List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        quoted = !quoted;
                    }
                }
                else if (ch == ',' && !quoted)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }
            values.Add(current.ToString());
            return values;
        }
    }
}
