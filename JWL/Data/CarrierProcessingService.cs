using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class CarrierProcessingService : ICarrierProcessingService
    {
        private readonly JWLDBContext _jWLDBContext;
        public CarrierProcessingService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddUser(users carrierusers, string UserID)
        {
            await _jWLDBContext.users.AddAsync(carrierusers);
            await _jWLDBContext.SaveChanges(UserID);
        }

        public async Task EditUser(users users, string UserID)
        {
            //_jWLDBContext.users.Update(users);
            users users1 = await _jWLDBContext.users.Where(t => t.userId == users.userId).FirstOrDefaultAsync();
            users1.userId = users.userId;
            users1.name = users.name;
            users1.email = users.email;
            users1.password = users.password;
            users1.userType = users.userType;
            users1.isFirstTime = users.isFirstTime;
            users1.isDeleted = users.isDeleted;
            users1.isActive = users.isActive;
            await _jWLDBContext.SaveChanges(UserID);
        }
        public async Task<bool> IsActive(int userID, string ProcessName)
        {
            var users = await _jWLDBContext.users.AsNoTracking().Where(t => t.userId == userID).FirstOrDefaultAsync();
            if (users != null && users.isActive == true && (users.isDeleted == null || users.isDeleted == false))
            {
                await _jWLDBContext.audit.AddAsync(new audit()
                {
                    auditUser = userID,
                    pageName = ProcessName,
                    auditDateTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<users> GetUsers(int id)
        {
            return await _jWLDBContext.users.AsNoTracking().Where(t => t.userId == id).FirstOrDefaultAsync();
        }

        public async Task<users> IsValidDetails(users users)
        {
            if (users != null && users.email != null
                && users.email != "" && users.password != null && users.password != "")
            {
                users IsValid = await _jWLDBContext.users.AsNoTracking().Where(t => t.email == users.email && t.password == users.password).FirstOrDefaultAsync();
                if (IsValid != null && IsValid.isActive == true && (IsValid.isDeleted == null || IsValid.isDeleted == false))
                {
                    return IsValid;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<List<users>> ListUsers()
        {
            return await _jWLDBContext.users.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }

        public async Task<users> IsValidateSSOUser(users users)
        {
            if (users != null && users.email != null
                && users.email != "")
            {
                users IsValid = await _jWLDBContext.users.AsNoTracking().Where(t => t.email == users.email).FirstOrDefaultAsync();
                if (IsValid != null && IsValid.isActive == true && (IsValid.isDeleted == null || IsValid.isDeleted == false))
                {
                    return IsValid;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public async Task<string> ValidateUserEmail(string emailId)
        {
            try
            {
                if (emailId != null)
                {
                    var Carriers = await _jWLDBContext.users.AsNoTracking().ToListAsync();
                    bool IsEmail = Carriers.Any(t => t.email.ToLower() == emailId.ToLower() && t.isActive == true);
                    return IsEmail.ToString();
                }
                else
                {
                    return "false";
                }

            }
            catch (Exception ex)
            {

                _jWLDBContext.errortracelog.Add(new errortracelog()
                {
                    error = ex.ToString(),
                    errorControl = "CarrierProcessingService/ValidateUserEmail",
                    errorMessage = ex.Message,
                    errorName = ex.Source,
                    errorStack = ex.StackTrace,
                    errorTime = DateTime.Now
                });
                await _jWLDBContext.SaveChangesAsync();
                return false.ToString();
            }
        }
    }
}
