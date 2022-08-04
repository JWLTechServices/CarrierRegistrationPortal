using Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IReportService
    {
        Task<List<audit>> GetReports();
        Task<List<audit>> GetReportsByUserID(int id);
        Task<List<audit>> GetReportsByDate(string from,string to,int id);
    }
}
