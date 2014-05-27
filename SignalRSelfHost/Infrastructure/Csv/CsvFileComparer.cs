namespace SignalRSelfHost.Infrastructure.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Web;

    using CsvHelper;

    using Newtonsoft.Json.Converters;

    public class CsvFileComparer
    {
        private const int ChangedRowLimit = 1000;

        public CsvFileComparer(CsvFileProcessor old, CsvFileProcessor @new)
        {
            if (old == null || old.LineCnt < 1)
            {
                throw new ArgumentException("Old");
            }

            if (@new == null || @new.LineCnt < 1)
            {
                throw new ArgumentException("New");
            }

            this.Old = old;
            this.New = @new;
            this.AddedRows = new Dictionary<string, string[]>();
            this.RemovedRows = new Dictionary<string, string[]>();
            this.ChangedRows = new Dictionary<string, string[]>();
        }

        public string KeyColumn { get; private set; }

        public CsvFileProcessor New { get; set; }

        public CsvFileProcessor Old { get; set; }

        public Dictionary<string, string[]> RemovedRows { get; private set; }

        public Dictionary<string, string[]> AddedRows { get; private set; }

        public Dictionary<string, string[]> ChangedRows { get; private set; }

        public bool HasManyChanges { get; private set; }

        public void Compare()
        {
            var removedHashes = this.Old.RowHashes.Where(rowHash => !this.New.RowHashes.ContainsKey(rowHash.Key)).ToList();
            if (removedHashes.Count > ChangedRowLimit)
            {
                this.HasManyChanges = true;
                return;
            }

            foreach (var removedHash in removedHashes)
            {
                var row = this.Old.GetRowByHash(removedHash.Key);
                if (this.New.Keys.ContainsKey(removedHash.Value))
                {
                    // key exists in new file -> changed row
                    this.ChangedRows[removedHash.Key] = row;
                }
                else
                {
                    this.RemovedRows[removedHash.Key] = row;
                }
            }

            var addedHashes = this.New.RowHashes.Where(rowHash => !this.Old.RowHashes.Keys.Contains(rowHash.Key)).ToList();
            if (addedHashes.Count > ChangedRowLimit)
            {
                this.HasManyChanges = true;
                return;
            }

            foreach (var addedHash in addedHashes)
            {
                var addedRow = this.New.GetRowByHash(addedHash.Key);
                if (!this.ChangedRows.ContainsKey(addedHash.Key))
                {
                    // only put row into AddedRows if it is not in ChangedRows already
                    this.AddedRows[addedHash.Key] = addedRow;
                }
                else
                {
                    Trace.WriteLine(addedHash.Value);
                }
            }
        }

        public string GetChangedRowsHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<style type=""text/css"">
                            table.gridtable {
	                            font-family: verdana,arial,sans-serif;
	                            font-size:11px;
	                            color:#333333;
	                            border-width: 1px;
	                            border-color: #666666;
	                            border-collapse: collapse;
                            }
                            table.gridtable th {
	                            border-width: 1px;
	                            padding: 8px;
	                            border-style: solid;
	                            border-color: #666666;
	                            background-color: #dedede;
                            }
                            table.gridtable td {
	                            border-width: 1px;
	                            padding: 8px;
	                            border-style: solid;
	                            border-color: #666666;
	                            background-color: #ffffff;
                            }
                            </style>");
            sb.AppendLine("<table class=\"gridtable\">");
            sb.AppendLine("<tr>");
            this.Old.HeaderFields.ToList().ForEach(h => sb.AppendFormat("<th>{0}</th>", h));

            sb.AppendLine("</tr>");

            var list = new List<string>();

            // add removed rows
            foreach (var removedRow in this.RemovedRows.OrderBy(r => r.Value[this.Old.GetDateIndex()]))
            {
                list.Add(this.CreateHtmlDiffRow(removedRow.Value, null));
            }

            // add changed rows
            foreach (var changedRow in this.ChangedRows.OrderBy(r => r.Value[this.Old.GetDateIndex()]))
            {
                var oldRow = this.Old.GetRowByHash(changedRow.Key);
                var keyValue = this.Old.GetKeyFromRow(oldRow);
                var newRow = this.New.GetRowByKey(keyValue);


                list.Add(this.CreateHtmlDiffRow(oldRow, newRow));
            }

            // add new rows
            foreach (var addedRow in this.AddedRows.OrderBy(r => r.Value[this.Old.GetDateIndex()]))
            {
                list.Add(this.CreateHtmlDiffRow(null, addedRow.Value));
            }

            list.ForEach(r => sb.AppendLine(r));
            if (list.Count > 2)
            {
                File.WriteAllText(@"c:\temp\difftest.htm", sb.ToString());
            }

            return sb.ToString();
        }

        private string CreateHtmlDiffRow(string[] rowOld, string[] rowNew)
        {
            var sb = new StringBuilder();
            sb.Append("<tr>");

            // all removed
            if (rowNew == null)
            {
                rowOld.ToList()
                    .ForEach(
                        f =>
                        sb.AppendFormat("<td><del style=\"background:#ffe6e6;\">{0}</del></td>", this.SanitizeField(f)));
            }

            // modified
            if (rowOld != null && rowNew != null)
            {
                if (this.Old.HeaderFields.Length != rowOld.Length)
                {
                    throw new InvalidOperationException("Number of row fields does not match number of header fields");
                }

                if (this.New.HeaderFields.Length != rowNew.Length)
                {
                    throw new InvalidOperationException("Number of row fields does not match number of header fields");
                }

                if (this.New.HeaderFields.Length != this.Old.HeaderFields.Length)
                {
                    throw new InvalidOperationException("Number of header fields does not match");
                }

                for (int i = 0; i < rowOld.Length; i++)
                {
                    if (rowOld[i] == rowNew[i])
                    {
                        sb.AppendFormat("<td>{0}</td>", this.SanitizeField(rowOld[i]));
                    }
                    else
                    {
                        sb.AppendFormat("<td><del style=\"background:#ffe6e6;\">{0}</del><br><ins style=\"background:#e6ffe6;\">{1}</ins></td>", this.SanitizeField(rowOld[i]), this.SanitizeField(rowNew[i]));
                    }
                }
            }

            // all added
            if (rowOld == null)
            {
                rowNew.ToList()
                    .ForEach(
                        f =>
                        sb.AppendFormat("<td><ins style=\"background:#e6ffe6;\">{0}</ins></td>", this.SanitizeField(f)));
            }

            sb.Append("</tr>");
            return sb.ToString();
        }

        private string SanitizeField(string rawFieldText)
        {
            rawFieldText = rawFieldText.Trim('"');
            return HttpUtility.HtmlEncode(rawFieldText);
        }
    }
}