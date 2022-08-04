using Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data
{
    public class AuditHelper
    {
        readonly IAuditDbContext Db;

        public AuditHelper(IAuditDbContext db)
        {
            Db = db;
        }

        public void AddAuditLogs(string userName)
        {
            Db.ChangeTracker.DetectChanges();
            List<AuditEntry> auditEntries = new List<AuditEntry>();
            foreach (EntityEntry entry in Db.ChangeTracker.Entries())
            {
                if (entry.Entity is audit || entry.State == EntityState.Detached ||
                    entry.State == EntityState.Unchanged)
                {
                    continue;
                }
                var auditEntry = new AuditEntry(entry, userName);
                auditEntries.Add(auditEntry);
            }

            if (auditEntries.Any())
            {
                var logs = auditEntries.Select(x => x.ToAudit()).ToList();
                for (int i = 0; i < logs.Count; i++)
                {
                    Db.audit.Add(logs[i]);
                }

            }
        }
    }
}
