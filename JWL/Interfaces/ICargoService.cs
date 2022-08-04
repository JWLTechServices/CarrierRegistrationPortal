using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ICargoService
    {
        Task<List<cargospecialties>> GetCargoSpecialities();
        Task<List<cargospecialties>> GetActiveCargoSpecialities();
        Task AddCargoSpecialities(cargospecialties cargospecialties, string UserID);
        Task<cargospecialties> GetCargo(int id);
        Task EditCargoSpecialities(cargospecialties cargospecialties, string UserID);
    }
}
