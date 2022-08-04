using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarrierDataPushing
{
    public  class clsJWLDBContext : DbContext
    {
        IConfiguration Config = new ConfigurationBuilder()
             .AddJsonFile("appSettings.json")
             .Build();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseMySQL(@"server=localhost;user=root;password=Admin@123;database=jwl");
            optionsBuilder.UseMySQL(Config.GetSection("ConnectionStrings")["DefaultConnection"]);
        }

        //public DbSet<Author> Authors { get; set; }
        public virtual DbSet<carrierusers> carrierusers { get; set; }

        //public virtual Task<int> SaveChanges(string userName)
        //{
        //    new AuditHelper(this).AddAuditLogs(userName);
        //    var result = await SaveChangesAsync();
        //    return result;
        //}
        public DbSet<errortracelog> errortracelog { get; set; }
        public virtual DbSet<OtpLogs> otplogs { get; set; }
        public virtual DbSet<cuusers> cuusers { get; set; }
        public virtual DbSet<state> state { get; set; }
    }
}
