using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Threading.Tasks;

namespace Data
{
    public class JWLDBContext : DbContext, IMopDbContext
    {
        public JWLDBContext(DbContextOptions<JWLDBContext> options)
            : base(options)
        {

        }

        public virtual DbSet<carrierusers> carrierusers { get; set; }
        public virtual DbSet<state> state { get; set; }
        public virtual DbSet<carriervehicle> carriervehicle { get; set; }
        public virtual DbSet<authorizedpath> authorizedpath { get; set; }
        public virtual DbSet<carrierTrailer> carrierTrailer { get; set; }
        public virtual DbSet<city> city { get; set; }
        public virtual DbSet<cargospecialties> cargospecialties { get; set; }
        public virtual DbSet<vehicltype> vehicltype { get; set; }
        public virtual DbSet<marketstate> marketstate { get; set; }
        public virtual DbSet<paymenttype> paymenttype { get; set; }
        public virtual DbSet<users> users { get; set; }
        public virtual DbSet<trailer> trailer { get; set; }
        //Added for Testing about audit trail
        public virtual DbSet<audit> audit { get; set; }
        public DbSet<errortracelog> errortracelog { get; set; }
        public async virtual Task<int> SaveChanges(string userName)
        {
            new AuditHelper(this).AddAuditLogs(userName);
            var result = await SaveChangesAsync();
            return result;
        }
        public virtual DbSet<OtpLogs> otplogs { get; set; }
        public virtual DbSet<cuusers> cuusers { get; set; }
    }
}
