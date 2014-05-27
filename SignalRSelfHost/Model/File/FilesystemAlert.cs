namespace SignalRSelfHost.Model.File
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using SignalRSelfHost.Infrastructure;
    using SignalRSelfHost.Infrastructure.Config;

    public class FilesystemAlert
    {
        [Key]
        public int Id { get; set; }

        public DomainId Domain { get; set; }

        public UniversityId University { get; set; }

        public AlertType AlertType { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Committed { get; set; }

        public string Message { get; set; }

        public string UploadConfigJson { get; set; }

        public int? FilesystemItemId { get; set; }

        public virtual FilesystemItem FilesystemItem { get; set; }
    }
}