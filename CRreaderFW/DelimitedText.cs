using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WindowsFormsApp1
{
    internal static class DelimitedText
    {
        public static List<string[]> Read(string path)
        {
            char delimiter = DetectDelimiter(path);
            var rows = new List<string[]>();
            foreach (string line in File.ReadLines(path, Encoding.Default))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                rows.Add(ParseLine(line, delimiter).ToArray());
            }

            return rows;
        }

        public static string ToCsvLine(IEnumerable<string> values)
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

        private static char DetectDelimiter(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension == ".tsv")
            {
                return '\t';
            }

            using (var reader = new StreamReader(path, Encoding.Default))
            {
                string line = reader.ReadLine() ?? string.Empty;
                int tabs = Count(line, '\t');
                int commas = Count(line, ',');
                int semicolons = Count(line, ';');
                if (tabs >= commas && tabs >= semicolons && tabs > 0)
                {
                    return '\t';
                }
                if (semicolons > commas)
                {
                    return ';';
                }
            }

            return ',';
        }

        private static int Count(string value, char needle)
        {
            int count = 0;
            foreach (char ch in value)
            {
                if (ch == needle)
                {
                    count++;
                }
            }

            return count;
        }

        private static List<string> ParseLine(string line, char delimiter)
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
                else if (ch == delimiter && !quoted)
                {
                    values.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            values.Add(current.ToString().Trim());
            return values;
        }
    }
}
