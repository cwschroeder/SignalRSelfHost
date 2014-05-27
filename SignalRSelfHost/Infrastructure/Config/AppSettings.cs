using System;
using System.Collections.Generic;
using SignalRSelfHost.Model;

namespace SignalRSelfHost.Infrastructure.Config
{
    public class WebSettings
    {
        public string BaseAddress { get; set; }
        public int WebApiPort { get; set; }
        public int SignalrPort { get; set; }
    }

    public class AuthSettings
    {
        public string AuthDomain { get; set; }

        public string AuthUser { get; set; }

        public string AuthPassword { get; set; }
    }


    // Upload
    // Directory 

    public class ObservedFile
    {
        // Regex for matching a file
        public string FileNamePattern { get; set; }

        // Allows check for file size
        public bool ShouldGrow { get; set; }

        // Allows checking of of CSV file rows for min/max date
        public string DateCheckColumnName { get; set; }

        // Pattern for performing date check 
        public string DateCheckPattern { get; set; }

        // Name of the unique key column
        public string KeyColumnName { get; set; }

        // Name of the 'Mandant' column
        public string ClientIdColumnName { get; set; }
    }

    public class ZipUploadConfig
    {
        public DomainId Domain { get; set; }

        public UniversityId University { get; set; }

        // Path where the uploaded zip file is stored
        public string UploadFolderPath { get; set; }

        // Path where the unzipped files are stored (same as UploadFolderPath)
        public string UnzipFolderPath { get; set; }

        // Defines the maximum interval between two subsequent zip
        public TimeSpan NewDataExpectedInterval { get; set; }

        public string ExcludeZipFilePattern { get; set; }

        public List<ObservedFile> ZipFiles { get; set; }

        public string ZipFileDatePattern { get; set; }
    }


    public class UploadObserverConfig
    {
        public List<ZipUploadConfig> ObservedDirectories { get; set; }
        
    }




    public class AppSettings
    {
        public AuthSettings Auth { get; set; }

        public WebSettings Web { get; set; }

        public UploadObserverConfig UploadObserver { get; set; }

    }
}