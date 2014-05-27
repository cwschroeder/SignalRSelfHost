namespace SignalRSelfHost.Model.File
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;

    [DebuggerDisplay("FullPath = {FullPath}")]
    public class DataDeliveryItem
    {
        [Key]
        public int Id { get; set; }

        public DomainId Domain { get; set; }

        public UniversityId University { get; set; }

        public string FullPath { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime TimeFromFileName { get; set; }

        public DateTime TimeFromFileAttribute { get; set; }

        // ForeignKey
        public int FilesystemItemId { get; set; }
    }
}