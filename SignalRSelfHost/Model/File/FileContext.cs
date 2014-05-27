using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalRSelfHost.Model.File
{
    public class FileContext : DbContext
    {
        public DbSet<FilesystemItem> FilesystemItems { get; set; }
        public DbSet<DataDeliveryItem> DataDelivery { get; set; }
        public DbSet<FilesystemAlert> FilesystemAlerts { get; set; } 

    }
}
