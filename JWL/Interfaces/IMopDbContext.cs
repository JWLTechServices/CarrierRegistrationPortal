using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IMopDbContext : IAuditDbContext, IDisposable
    {
        DbSet<carrierusers> carrierusers { get; set; }
        DbSet<state> state { get; set; }
        DatabaseFacade Database { get; }
        DbSet<carriervehicle> carriervehicle { get; set; }
        DbSet<authorizedpath> authorizedpath { get; set; }
        DbSet<carrierTrailer> carrierTrailer { get; set; }
        DbSet<city> city { get; set; }
        DbSet<cargospecialties> cargospecialties { get; set; }
        DbSet<vehicltype> vehicltype { get; set; }
        DbSet<marketstate> marketstate { get; set; }
        DbSet<paymenttype> paymenttype { get; set; }
        DbSet<users> users { get; set; }
        DbSet<trailer> trailer { get; set; }
        Task<int> SaveChanges(string userName);
    }
}
