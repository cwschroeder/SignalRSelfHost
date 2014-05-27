using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting.Starter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalRSelfHost.Model;

namespace SignalRSelfHost.Infrastructure.Config
{
    public class JsonConfig
    {
        private const string filePath = "settings.conf";

        public static AppSettings AppSettings
        {
            get { return Read(); }
        }

        public static AppSettings Read()
        {
            var text = File.ReadAllText(filePath);
            var c = JsonConvert.DeserializeObject<AppSettings>(text);
            return c;
        }

        public static void Write(AppSettings settings)
        {
            var text = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(filePath, text);
        }

        public static void Init()
        {
            Console.WriteLine("Initializing settings...");
            var settings = JsonConfig.Read();
            if (string.IsNullOrEmpty(settings.Auth.AuthUser))
            {
                Console.WriteLine("Enter domain user:");
                settings.Auth.AuthUser = Console.ReadLine();
                Write(settings);
            }

            if (string.IsNullOrEmpty(settings.Auth.AuthPassword))
            {
                Console.WriteLine("Enter domain password for user {0}:", settings.Auth.AuthUser);
                string pwd = string.Empty;
                ConsoleKeyInfo key;
                do
                {
                    key = Console.ReadKey(true);
                    if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                    {
                        pwd += key.KeyChar;
                        Console.Write("*");
                    }
                    else
                    {
                        if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
                        {
                            pwd = pwd.Substring(0, (pwd.Length - 1));
                            Console.Write("\b \b");
                        }
                    }
                } while (key.Key != ConsoleKey.Enter);

                settings.Auth.AuthPassword = StringCipher.Encrypt(pwd, "nsa");
                Write(settings);
            }

            if (settings.Web == null)
            {
                settings.Web = new WebSettings();
                settings.Web.BaseAddress = "http://localhost";
                settings.Web.SignalrPort = 9000;
                settings.Web.WebApiPort = 8080;
                Write(settings);
            }

            if (settings.UploadObserver == null)
            {
                settings.UploadObserver = new UploadObserverConfig()
                {
                    ObservedDirectories = new List<ZipUploadConfig>()
                };

                settings.UploadObserver.ObservedDirectories.Add(new ZipUploadConfig()
                {
                    Domain = DomainId.Finanzcontrolling,
                    University = UniversityId.Regensburg,
                    UploadFolderPath = @"\\ceus-etl2\c$\Daten\upload_from_web\KDV\scc39979",
                    NewDataExpectedInterval = TimeSpan.FromHours(25),
                    ZipFiles = new List<ObservedFile>()
                                        {
                                            new ObservedFile()
                                                {
                                                    FileNamePattern = @"(^.*)(Einzelbuchung_)(\d+)(\.csv)",
                                                    DateCheckColumnName = "buchungsdatum",
                                                    ShouldGrow = true
                                                }
                                        },
                    ExcludeZipFilePattern = @"(^upload_klr)(.*)",
                    ZipFileDatePattern = @"Upload_FC_yyyy-MM-dd-HH-mm-ss-ff"
                }); 
                
                Write(settings);
            }

        }
    }
}
