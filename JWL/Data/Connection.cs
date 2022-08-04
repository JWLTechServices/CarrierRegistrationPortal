using Microsoft.Extensions.Configuration;
using System.IO;

namespace Data
{
    public static class Connection
    {
        public static string ConnectionString()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            return configuration.GetConnectionString("DefaultConnection");
        }

    }
}
