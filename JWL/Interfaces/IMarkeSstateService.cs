using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IMarkeSstateService
    {
        Task<List<marketstate>> GetMarketState();
        Task<List<marketstate>> GetActiveMarketState();
        Task AddMarketState(marketstate marketstate,string UserID);
        Task<marketstate> GetMarketstate(int id);
        Task EditMarketstate(marketstate marketstate, string UserID);
    }
}