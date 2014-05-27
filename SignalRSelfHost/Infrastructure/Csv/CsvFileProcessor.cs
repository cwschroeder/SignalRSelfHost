using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRSelfHost.Infrastructure.Csv
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;

    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    public class CsvFileProcessor
    {
        public int LineCnt
        {
            get
            {
                return this.csv.Parser.Row -1;
            }
        }

        public DateTime LastRowAt { get; set; }

        public DateTime FirstRowAt { get; set; }

        public Dictionary<string, int> RowHashes { get; private set; }

        public string[] HeaderFields { get; set; }

        public Dictionary<int, string[]> Rows { get; set; }

        public Dictionary<int, string> Keys { get; set; } 

        private string fullPath;

        private CsvReader csv;

        private static DateTime MaxDateTime = new DateTime(2100, 1, 1);

        private static DateTime MinDateTime = new DateTime(1970, 1, 1);

        private string keyColumnName;

        private string dateColumnName;

        public CsvFileProcessor(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException(fullPath);
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(fullPath);
            }

            this.fullPath = fullPath;

            this.csv = new CsvReader(
                 new StreamReader(this.fullPath),
                 new CsvConfiguration()
                 {
                     Delimiter = ";",
                     HasHeaderRecord = true,
                     QuoteAllFields = true,
                     Encoding = Encoding.UTF8
                 });

            this.FirstRowAt = MaxDateTime;
            this.LastRowAt = MinDateTime;
        }

        public bool Parse(string dateCheckColumnName, string dateCheckPattern, string keyColumnName)
        {
            this.keyColumnName = keyColumnName;
            this.dateColumnName = dateCheckColumnName;
            this.RowHashes = new Dictionary<string, int>();
            this.Rows = new Dictionary<int, string[]>();
            this.Keys = new Dictionary<int, string>();

            // read first line to get headers
            if (!this.csv.Read())
            {
                return false;
            }

            this.HeaderFields = this.csv.FieldHeaders;
            if (!string.IsNullOrEmpty(keyColumnName))
            {
                if (!this.HeaderFields.Contains(keyColumnName))
                {
                    throw new InvalidOperationException("Key column " + keyColumnName + " not found");
                }
            }

            var hasher = new MD5CryptoServiceProvider();
            while (this.csv.Read())
            {
                // store row
                this.Rows[this.LineCnt] = this.csv.CurrentRecord;

                // calculate row hashes
                var hashedLine = BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(this.csv.Parser.RawRecord))).Replace("-", string.Empty);
                this.RowHashes[hashedLine] = this.LineCnt;

                // check min/max dates
                if (!string.IsNullOrEmpty(dateCheckColumnName) && !string.IsNullOrEmpty(dateCheckPattern))
                {
                    this.UpdateDates(dateCheckColumnName, dateCheckPattern);
                }

                if (!string.IsNullOrEmpty(keyColumnName))
                {
                    this.UpdateKeys(keyColumnName);
                }
            }

            if (this.FirstRowAt == MaxDateTime)
            {
                // reset initial value to min value
                this.FirstRowAt = MinDateTime;
            }

            return true;
        }

        public string[] GetRowByHash(string hash)
        {
            var rowIndex = this.RowHashes.First(h => h.Key == hash).Value;
            var row = this.Rows[rowIndex];
            return row;
        }

        public string[] GetRowByKey(string key)
        {
            var keyIndex = this.Keys.First(v => v.Value == key).Key;
            var row = this.Rows[keyIndex];
            return row;
        }

        public string GetKeyFromRow(string[] row)
        {
            if (string.IsNullOrEmpty(this.keyColumnName))
            {
                throw new NullReferenceException("No key column specified");
            }

            var fieldIndex = this.HeaderFields.ToList().FindIndex(h => h == this.keyColumnName);
            return row[fieldIndex];
        }

        public int GetKeyIndex()
        {
            if (string.IsNullOrEmpty(this.keyColumnName))
            {
                return 0;
            }

            return this.HeaderFields.ToList().FindIndex(h => h == this.keyColumnName);
        }

        public int GetDateIndex()
        {
            if (string.IsNullOrEmpty(this.dateColumnName))
            {
                return 0;
            }

            return this.HeaderFields.ToList().FindIndex(h => h == this.dateColumnName);
        }

        public int GetMandantIndex()
        {
            return this.HeaderFields.ToList().FindIndex(h => h.ToLowerInvariant().StartsWith("mandant"));
        }

        private void UpdateDates(string columnName, string datePattern)
        {
            var dateField = this.csv.GetField<string>(columnName);
            var parsedDate = DateTime.ParseExact(dateField, datePattern, null);
            if (parsedDate < this.FirstRowAt)
            {
                this.FirstRowAt = parsedDate;
            }

            if (parsedDate > this.LastRowAt)
            {
                this.LastRowAt = parsedDate;
            }
        }

        private void UpdateKeys(string columnName)
        {
            var keyField = this.csv.GetField<string>(columnName);
            this.Keys[this.LineCnt] = keyField;
        }
    }
}
