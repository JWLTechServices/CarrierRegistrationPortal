using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IVehicleTypeService
    {
        Task<List<vehicltype>> GetVehicleTypes();
        Task<List<vehicltype>> GetActiveVehicleTypes();
        Task AddVehicleTypes(vehicltype vehicltype,string UserID);
        Task<vehicltype> GetVehicleTypes(int id);
        Task EditVehicleTypes(vehicltype vehicltype, string UserID);
    }
}
