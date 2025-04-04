using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Data
{
    ///This is used for quick migration => uncomment this to use
    //public class EStoreDbContextFactory : IDesignTimeDbContextFactory<EStoreDbContext>
    //{
    //    public EStoreDbContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<EStoreDbContext>();

    //        // IMPORTANT: Match this with your real connection string
    //        const string connectionString = "Data Source=localhost,1433;Initial Catalog=eStoreDb;User ID=sa;Password=1234567890;TrustServerCertificate=True;MultipleActiveResultSets=true";

    //        optionsBuilder.UseSqlServer(connectionString);

    //        return new EStoreDbContext(optionsBuilder.Options);
    //    }
    //}
}
