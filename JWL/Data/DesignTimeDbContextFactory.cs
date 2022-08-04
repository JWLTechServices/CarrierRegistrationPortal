using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<JWLDBContext>
    {
        public JWLDBContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var builder = new DbContextOptionsBuilder<JWLDBContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            builder.UseMySQL(connectionString);
            return new JWLDBContext(builder.Options);
        }
    }
}
