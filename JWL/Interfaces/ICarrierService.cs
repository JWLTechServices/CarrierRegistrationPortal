using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICarrierService
    {
        Task<List<carrierusers>> GetCarrierUsers();
        Task<List<carrierusers>> ExportCarrierUsers();
        Task AddCarrierUser(carrierusers carrierUser, string UserID, string defaultPassword = "", string passwordSalt = "");
        Task UpdateURL(carrierusers carrierusers, string UserID);
        Task<carrierusers> GetCarrierusersById(int id);
        Task EditCarrierUser(carrierusers carrierUser, string UserID);
        Task<string> CheckEmailDOT(string Email, string DOT, bool IsEdit, int id = 0);
        Task<string> CheckDOT(string DOT, bool IsEdit, int id = 0);
        Task<string> CheckEmail(string Email, bool IsEdit, int id = 0);
        Task DeleteAttachment(string File, string UserID);
        Task DeleteVehicle(int VehicleID, string UserID);
        Task DeleteTrailer(int TrailerID, string UserID);
        Task<cuusers> IsValidCarrierUserDetails(cuusers carrierusers);
        Task<bool> IsCuuserActive(int userID, string ProcessName);
        Task<cuusers> GetCuuser(int id);
        Task<cuusers> GetCuuserByCuId(int cuId);
        Task<cuusers> GetCuuserByEmailId(string emailId);
        Task EditCuuser(cuusers users, string UserID);
        Task<string> GenerateNumericOTP(int OtpLength);
        Task InsertOtp(OtpLogs otpLogs);
        Task<OtpLogs> GetOtpLogsDetails(int otp, string token);
        Task<OtpLogs> GetOtpLogsTokenDetails(string token);
        Task<OtpLogs> GetOtpDetails(int otp);
        Task<string> CheckSCACCode(string scacCode, bool IsEdit, int id = 0);
        Task<string> CheckDataTrac_UID(string dtuid, bool IsEdit, int id = 0);

    }
}
