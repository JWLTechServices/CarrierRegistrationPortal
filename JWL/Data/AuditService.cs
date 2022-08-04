using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class AuditService : IAuditService
    {
        private readonly JWLDBContext _jWLDBContext;

        public AuditService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddAudit(audit audit)
        {
            await _jWLDBContext.audit.AddAsync(audit);
            await _jWLDBContext.SaveChangesAsync();
        }
    }
}
