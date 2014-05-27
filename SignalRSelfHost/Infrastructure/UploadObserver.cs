using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Hosting;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NLog.LayoutRenderers;
using NLog.Targets;
using SignalRSelfHost.Infrastructure.Config;
using SignalRSelfHost.Model.File;

namespace SignalRSelfHost.Infrastructure
{
    using System.CodeDom;
    using System.Data.Entity;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using CsvHelper;
    using CsvHelper.Configuration;

    using Newtonsoft.Json;

    using NLog.Config;

    using SignalRSelfHost.Infrastructure.Csv;

    /// <summary>
    /// Monitoring of periodic data deliveries:
    /// - Check if data delivery (zip-archive) is present within the configured time interval in the configured folder
    /// - Exclude files that should not regarded as data deliveries
    /// - Check if all configured delivery items (CSV-files) are present
    /// - Collect file statistics (CreatedAt timestamp, File-Size, File-Name, Status, LastCheckedAt, Type, etc.)
    /// - Check contents of CSV-File
    ///     - Check for changes of first and last row according to a named date column and a configurable date pattern (optional)
    ///     - Check for changes according number of rows
    ///     - Check for changes on field level (finds changed rows by a configurable unique key column)
    ///     - Collect change statistics (number of total rows, rows removed, changed, added)
    ///     - Check if file size is growing constantly
    /// </summary>
    public class UploadObserver
    {
        private readonly NetworkCredential netCredential;
        
        public UploadObserver()
        {
            this.netCredential = new NetworkCredential(
                JsonConfig.AppSettings.Auth.AuthUser,
                StringCipher.Decrypt(JsonConfig.AppSettings.Auth.AuthPassword, "nsa"),
                JsonConfig.AppSettings.Auth.AuthDomain);
        }

        protected List<FilesystemItem> FilesystemItems { get; set; }

        protected List<DataDeliveryItem> DataDeliveries { get; set; }


        public async void Run()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    this.RefreshEntities();

                    // get latest config
                    var observedDirectories = JsonConfig.AppSettings.UploadObserver.ObservedDirectories;
                    this.CheckDirs(observedDirectories);

                    await Task.Delay(5000);
                    Console.WriteLine(@"\n\n\Filesystemobserver next run... ");
                }
            });
        }

        private void RefreshEntities()
        {
            using (var db = new FileContext())
            {
                this.FilesystemItems = db.FilesystemItems.ToList();
                this.DataDeliveries = db.DataDelivery.ToList();
            }
        }

        private void CheckDirs(List<ZipUploadConfig> observedDirectories)
        {
            foreach (var observedDir in observedDirectories)
            {
                this.CheckObservedDir(observedDir);
            }
        }

        private void CheckObservedDir(ZipUploadConfig uploadConfig)
        {
            // connect to remote client upload directory
            using (new NetworkConnection(uploadConfig.UploadFolderPath, this.netCredential))
            {
                // folder where the zip files are stored
                var uploadDir = new DirectoryInfo(uploadConfig.UploadFolderPath);

                // get all relevant zip files within the upload folder
                var zipFiles = uploadDir.GetFileSystemInfos("*.zip").Where(zipFile => !this.IsZipExcluded(zipFile.Name, uploadConfig)).ToList();
                foreach (var zipFile in zipFiles)
                {
                    // create data delivery item if neccessary
                    var dataDeliveryItem = this.GetOrCreateDataDeliveryItem(zipFile.FullName, uploadConfig);
                    if (!this.DataDeliveries.Contains(dataDeliveryItem))
                    {
                        this.AddDataDelivery(dataDeliveryItem, uploadConfig);
                    }
                }

                // RULE: check for the presence of new zip file within the expected interval
                var lastZip = zipFiles.OrderByDescending(z => z.CreationTime).FirstOrDefault();
                if (DateTime.Now - uploadConfig.NewDataExpectedInterval > lastZip.CreationTime)
                {
                    this.RaiseAlert(AlertType.NoUploadWithinExpectedInterval, uploadConfig, null);
                }


                // RULE: check for the presence of a subDir for each zip file
                var unzipDirs = uploadDir.GetDirectories().Where(dir => !this.IsZipExcluded(dir.Name, uploadConfig)).ToList();
                var missingUnzipDirs = zipFiles.Select(z => z.Name.Substring(0, z.Name.Length - 4)).Except(unzipDirs.Select(d => d.Name)).ToList();
                foreach (var missingDir in missingUnzipDirs)
                {
                    // TODO: check if zip was uploaded just now, then wait some seconds and check again later
                    var zipFileWithoutFolder = Path.Combine(uploadDir.FullName, missingDir + ".zip");
                    this.RaiseAlert(AlertType.UnzipDirMissing, uploadConfig, this.GetOrCreateFilesystemItem(zipFileWithoutFolder, uploadConfig));
                }

                // inspect unzipped folder content for CSV files
                foreach (var unzipDir in unzipDirs)
                {
                    // store entry in database
                    var dirItem = this.GetOrCreateFilesystemItem(unzipDir.FullName, uploadConfig);
                    this.AddOrUpdateFilesystemItem(dirItem);

                    // descend one level down to get unzipped files
                    foreach (var file in unzipDir.GetFiles())
                    {
                        var fileItem = this.GetOrCreateFilesystemItem(file.FullName, uploadConfig);

                        // inspect file details if it is a new CSV file only
                        if (fileItem.Status != FileStatus.Added || fileItem.FileType != FileType.CsvFile)
                        {
                            continue;
                        }

                        this.AddOrUpdateFilesystemItem(fileItem);

                        // compare new file with last one
                        var predecessor = this.GetPredecessorItem(fileItem, uploadConfig);
                        if (predecessor != null)
                        {
                            // calculate Diff
                            //var diff = this.GetDiff(predecessor.FullPath, fileItem.FullPath);
                            //if (!string.IsNullOrEmpty(diff))
                            //{
                            //    File.WriteAllText(@"c:\temp\difftest.htm", diff);
                            //}
                        }
                    }
                }
            }
        }

        private DataDeliveryItem GetOrCreateDataDeliveryItem(string fullPath, ZipUploadConfig uploadConfig)
        {
            var zipFile = this.GetOrCreateFilesystemItem(fullPath, uploadConfig);
            if (zipFile.FileType != FileType.ZipFile)
            {
                throw new InvalidOperationException("Unexptected FileType: FilesystemItem for DataDeliverItem must be of FileType ZipFile!");
            }

            if (zipFile.Status == FileStatus.Added)
            {
                this.AddOrUpdateFilesystemItem(zipFile);
            }

            var dataDelivery = this.DataDeliveries.FirstOrDefault(d => d.FullPath == fullPath);
            if (dataDelivery == null)
            {
                dataDelivery = new DataDeliveryItem()
                                   {
                                       FullPath = fullPath,
                                       Domain = uploadConfig.Domain,
                                       University = uploadConfig.University,
                                       TimeFromFileAttribute = File.GetCreationTime(fullPath),
                                       TimeFromFileName =
                                           this.GetDateFromFileName(
                                               fullPath,
                                               uploadConfig.ZipFileDatePattern),
                                       FilesystemItemId = zipFile.Id
                                   };
            }

            return dataDelivery;
        }

        private DateTime GetDateFromFileName(string fullPath, string pattern)
        {
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            DateTime parsedDateTime;
            if (!DateTime.TryParseExact(fileName, pattern, null, DateTimeStyles.AssumeLocal, out parsedDateTime))
            {
                parsedDateTime = new DateTime(1970, 1, 1);
            }

            // remove invalid fractal seconds
            parsedDateTime = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day, parsedDateTime.Hour, parsedDateTime.Minute, parsedDateTime.Second);
            return parsedDateTime;
        }

        private string GetDatasetName(string fullPath, string pattern)
        {
            var match = Regex.Match(Path.GetFileName(fullPath), pattern);
            if (match.Groups.Count > 2)
            {
                return match.Groups[2].Value;
            }

            return string.Empty;
        }

        private void RaiseEvent(EventType eventType, ZipUploadConfig config, FilesystemItem item)
        {
            //TODO
        }

        private void RaiseAlert(AlertType alertType, ZipUploadConfig config, FilesystemItem item)
        {
            string message;
            switch (alertType)
            {
                case AlertType.UnzipDirMissing:
                    message = "The unzip folder is missing.";
                    break;
                case AlertType.CsvFileSizeNotGrown:
                    message = "The size of the csv file has not increased compared to the last file version.";
                    break;
                case AlertType.CsvMinDateIncreased:
                    message = "The min date of the csv file has increased compared to the last file version.";
                    break;
                case AlertType.CsvMaxDateDecreased:
                    message = "The max date of the csv file has decreased compared to the last file version.";
                    break;
                case AlertType.NoUploadWithinExpectedInterval:
                    message = "No upload was found within the configured interval.";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("alertType");
            }

            var alert = new FilesystemAlert()
                            {
                                AlertType = alertType,
                                CreatedAt = DateTime.Now,
                                Domain = config.Domain,
                                FilesystemItem = item,
                                University = config.University,
                                FilesystemItemId = item.Id,
                                UploadConfigJson = JsonConvert.SerializeObject(config),
                                Message = message
                            };

            using (var db = new FileContext())
            {
                db.FilesystemAlerts.Add(alert);
                db.SaveChanges();
            }
        }

        private FilesystemItem GetOrCreateFilesystemItem(string fullPath, ZipUploadConfig uploadConfig)
        {
            var fileType = this.GetFileType(fullPath);
            long fileSize = 0;
            FilesystemItem parentItem = null;
            if (fileType != FileType.Folder)
            {
                var fi = new FileInfo(fullPath);
                fileSize = fi.Length;
                if (fileType != FileType.ZipFile)
                {
                    var di = new DirectoryInfo(fullPath);
                    var zips = this.FilesystemItems.Where(i => i.FullPath == di.Parent.FullName + ".zip").ToList();
                    if (zips.Count > 1)
                    {
                        throw new Exception(string.Format("Zip archive {0} exists multiple times", di.Parent.FullName + ".zip"));
                    }

                    if (zips.Count == 0)
                    {
                        throw new Exception(string.Format("No Zip archive found for item {0}.", di.FullName));
                    }

                    parentItem = zips.FirstOrDefault();
                }
            }

            var creationTime = File.GetCreationTime(fullPath);
            var item = this.FilesystemItems.FirstOrDefault(i => i.FullPath == fullPath);
            if (item == null)
            {
                var fileItem = new FilesystemItem(
                    uploadConfig.Domain,
                    uploadConfig.University,
                    fullPath,
                    fileSize,
                    creationTime,
                    fileType);
                if (parentItem != null)
                {
                    fileItem.ParentItemId = parentItem.Id;
                }

                if (fileType == FileType.CsvFile && this.IsCsvIncluded(Path.GetFileName(fullPath), uploadConfig))
                {
                    var obsFile = this.GetObservedFileConfig(Path.GetFileName(fullPath), uploadConfig);

                    // get line count
                    var csvParser = new CsvFileProcessor(fileItem.FullPath);
                    csvParser.Parse(obsFile.DateCheckColumnName, obsFile.DateCheckPattern, obsFile.KeyColumnName);

                    fileItem.CsvLines = csvParser.LineCnt;
                    fileItem.FirstRowAt = csvParser.FirstRowAt;
                    fileItem.LastRowAt = csvParser.LastRowAt;

                    var lastItem = this.GetPredecessorItem(fileItem, uploadConfig);
                    if (lastItem != null)
                    {
                        // RULE: check datetime of first and last row
                        if (fileItem.FirstRowAt > lastItem.FirstRowAt)
                        {
                            this.RaiseAlert(AlertType.CsvMinDateIncreased, uploadConfig, fileItem);
                        }

                        if (fileItem.LastRowAt < lastItem.LastRowAt)
                        {
                            this.RaiseAlert(AlertType.CsvMaxDateDecreased, uploadConfig, fileItem);
                        }

                        // parse predecessor item for csv comparison
                        var csvParserOld = new CsvFileProcessor(lastItem.FullPath);
                        csvParserOld.Parse(obsFile.DateCheckColumnName, obsFile.DateCheckPattern, obsFile.KeyColumnName);
                        var comparer = new CsvFileComparer(csvParserOld, csvParser);
                        comparer.Compare();
                        if (!comparer.HasManyChanges && ((comparer.AddedRows.Count > 1) || (comparer.ChangedRows.Count > 1)))
                        {
                            fileItem.DiffHtml = comparer.GetChangedRowsHtml();
                        }

                        // add statistical data 
                        fileItem.RemovedRowCount = comparer.RemovedRows.Count;
                        fileItem.ChangedRowCount = comparer.ChangedRows.Count;
                        fileItem.AddedRowCount = comparer.AddedRows.Count;
                        fileItem.HasManyChanges = comparer.HasManyChanges;
                    }
                }

                return fileItem;
            }

            return item;
        }

        private FilesystemItem GetPredecessorItem(FilesystemItem item, ZipUploadConfig config)
        {
            if (!this.IsCsvIncluded(Path.GetFileName(item.FullPath), config))
            {
                return null;
            }

            var currentDataDelivery = this.DataDeliveries.FirstOrDefault(d => d.FilesystemItemId == item.ParentItemId);
            if (currentDataDelivery == null)
            {
                throw new NullReferenceException(string.Format("Could not get data delivery item for file {0}.", item.FullPath));
            }

            var lastDataDeliveries =
                this.DataDeliveries.Where(
                    d =>
                    d.Domain == item.Domain
                    && d.University == item.University
                    && d.TimeFromFileAttribute < currentDataDelivery.TimeFromFileAttribute).OrderByDescending(d => d.TimeFromFileAttribute).ToList();

            var lastDelivery = lastDataDeliveries.FirstOrDefault();
            if (lastDelivery == null)
            {
                return null;
            }

            // get corresponding file from last data delivery
            var obsFile = this.GetObservedFileConfig(Path.GetFileName(item.FullPath), config);
            var pattern = obsFile.FileNamePattern;
            var dataSetName = this.GetDatasetName(item.FullPath, pattern);
            var lastItem = this.FilesystemItems.FirstOrDefault(
                f =>
                    f.ParentItemId == lastDelivery.FilesystemItemId &&
                this.GetDatasetName(f.FullPath, obsFile.FileNamePattern) == dataSetName);

            return lastItem;
        }


        private FileType GetFileType(string fullPath)
        {
            FileType fileType;
            var attr = File.GetAttributes(fullPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                fileType = FileType.Folder;
            }
            else
            {
                switch (Path.GetExtension(fullPath))
                {
                    case ".csv":
                        fileType = FileType.CsvFile;
                        break;
                    case ".zip":
                        fileType = FileType.ZipFile;
                        break;
                    default:
                        fileType = FileType.Other;
                        break;
                }
            }

            return fileType;
        }

        private bool IsZipExcluded(string fileOrFolderName, ZipUploadConfig uploadConfig)
        {
            return Regex.IsMatch(fileOrFolderName, uploadConfig.ExcludeZipFilePattern);
        }

        private bool IsCsvIncluded(string fileName, ZipUploadConfig uploadConfig)
        {
            return uploadConfig.ZipFiles.Any(f => Regex.IsMatch(fileName, f.FileNamePattern));
        }

        private ObservedFile GetObservedFileConfig(string fileName, ZipUploadConfig uploadConfig)
        {
            return uploadConfig.ZipFiles.FirstOrDefault(f => Regex.IsMatch(fileName, f.FileNamePattern));
        }

        private void AddOrUpdateFilesystemItem(FilesystemItem item)
        {
            item.Status = this.FilesystemItems.Any(i => i.FullPath == item.FullPath) ? FileStatus.Unchanged : FileStatus.Added;

            using (var db = new FileContext())
            {
                if (item.Status == FileStatus.Added)
                {
                    db.FilesystemItems.Add(item);
                    this.FilesystemItems.Add(item);
                }
                else
                {
                    item.LastCheckedAt = DateTime.Now;
                    db.FilesystemItems.Attach(item);
                }

                db.SaveChanges();
            }

            Console.WriteLine("{0} - {1}", item.Status, item.FullPath);
        }


        private void AddDataDelivery(DataDeliveryItem item, ZipUploadConfig config)
        {
            // add or update FilesystemItem (ZIP-File)
            var fs = this.GetOrCreateFilesystemItem(item.FullPath, config);
            this.AddOrUpdateFilesystemItem(fs);

            // add new data delivery item
            using (var db = new FileContext())
            {
                item.CreatedAt = DateTime.Now;
                db.DataDelivery.Add(item);
                this.DataDeliveries.Add(item);
                db.SaveChanges();
            }
        }
    }
}
