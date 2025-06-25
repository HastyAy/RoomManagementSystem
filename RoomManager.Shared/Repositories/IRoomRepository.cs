using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomManager.Shared.Entities;


namespace RoomManager.Shared.Repositories
{
  
    public interface IRoomRepository
    {
        Task<List<Room>> GetAllAsync();
        Task<Room?> GetByIdAsync(Guid id);
        Task AddAsync(Room room);
        Task UpdateAsync(Room room);
        Task DeleteAsync(Guid id);
    }

}
