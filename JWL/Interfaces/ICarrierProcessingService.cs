using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICarrierProcessingService
    {
        Task<users> IsValidDetails(users carrierusers);
        Task<List<users>> ListUsers();
        Task AddUser(users carrierusers, string UserID);
        Task<users> GetUsers(int id);
        Task EditUser(users users, string UserID);
        Task<bool> IsActive(int userID,string ProcessName);
        Task<users> IsValidateSSOUser(users carrierusers);
        Task<string> ValidateUserEmail(string emailId);
    }
}
