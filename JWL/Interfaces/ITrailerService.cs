using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface ITrailerService
    {
        Task<List<trailer>> GetTrailer();
        Task<List<trailer>> GetActiveTrailer();
        Task AddTrailer(trailer trailer, string UserID);
        Task<trailer> GetTrailer(int id);
        Task EditTrailer(trailer trailer, string UserID);
    }
}
