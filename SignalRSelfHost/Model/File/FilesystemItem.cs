using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using SignalRSelfHost.Infrastructure;

namespace SignalRSelfHost.Model.File
{
    using System.Diagnostics;

    [DebuggerDisplay("FullPath = {FullPath}")]
    public class FilesystemItem
    {
        public FilesystemItem()
        {
        }

        public FilesystemItem(DomainId domain, UniversityId university, string path, long size, DateTime createdAt, FileType fileType)
        {
            this.Domain = domain;
            this.University = university;

            this.FullPath = path;
            this.Size = size;
            this.CreatedAt = createdAt;
            this.FileType = fileType;
            this.LastCheckedAt = DateTime.Now;
            this.FirstRowAt = new DateTime(1970, 1, 1);
            this.LastRowAt = new DateTime(1970, 1, 1);
            this.Status = FileStatus.Added;
        }

        [Key]
        public int Id { get; set; }

        // ZipUploadConfig items
        public DomainId Domain { get; set; }

        public UniversityId University { get; set; }

        public string FullPath { get; set; }

        public long Size { get; set; }

        public int? CsvLines { get; set; }

        /// <summary>
        /// File or Folder create timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        public DateTime FirstRowAt { get; set; }

        public DateTime LastRowAt { get; set; }

        public FileType FileType { get; set; }

        public FileStatus Status { get; set; }

        public DateTime LastCheckedAt { get; set; }

        // corresponding mother zip file if available
        public int ParentItemId { get; set; }

        public string DiffHtml { get; set; }

        // statistical data
        public int RemovedRowCount { get; set; }

        public int ChangedRowCount { get; set; }

        public int AddedRowCount { get; set; }

        public bool HasManyChanges { get; set; }
    }
}