using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data
{
    public class StateService : IStateService
    {
        private readonly JWLDBContext _jWLDBContext;

        public StateService(JWLDBContext jWLDBContext)
        {
            _jWLDBContext = jWLDBContext;
        }

        public async Task AddState(state state, string UserID)
        {
            await _jWLDBContext.state.AddAsync(state);
            await _jWLDBContext.SaveChanges(UserID);
        }
        public async Task EditState(state state, string UserID)
        {

            var states = await _jWLDBContext.state.Where(t => t.stateId == state.stateId).FirstOrDefaultAsync();
            states.stateName = state.stateName;
            states.isActive = state.isActive;
            states.isDeleted = state.isDeleted;
            //_jWLDBContext.state.Update(states);
            await _jWLDBContext.SaveChanges(UserID);
        }
        public async Task<state> GetState(int id)
        {
            return await _jWLDBContext.state.AsNoTracking().Where(t => t.stateId == id).FirstOrDefaultAsync();
        }

        public async Task<List<state>> GetActiveStates()
        {
            return await _jWLDBContext.state.AsNoTracking().Where(t => (t.isDeleted == null || t.isDeleted == false) && (t.isActive == true)).ToListAsync();
        }
        public async Task<List<state>> GetStates()
        {
            return await _jWLDBContext.state.AsNoTracking().Where(t => t.isDeleted == null || t.isDeleted == false).ToListAsync();
        }
    }
}
