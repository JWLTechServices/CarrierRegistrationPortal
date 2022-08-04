using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IAuditService
    {
        Task AddAudit(audit audit);
    }
}
