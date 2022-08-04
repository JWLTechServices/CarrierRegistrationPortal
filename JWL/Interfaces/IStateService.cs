using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IStateService
    {
        Task<List<state>> GetStates();
        Task<List<state>> GetActiveStates();
        Task AddState(state state,string UserID);
        Task<state> GetState(int id);
        Task EditState(state state, string UserID);
    }
}
