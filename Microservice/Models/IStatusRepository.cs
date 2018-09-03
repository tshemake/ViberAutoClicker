using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Models
{
    public interface IStatusRepository : IDisposable
    {
        Task<ICollection<Status>> GetAllAsync();

        Task<Status> GetByIdAsync(Guid id);

        Task<int> AddAsync(Status status);

        Task<int> DeleteByIdAsync(Guid id);

        Task<int> UpdateAsync(Status status);

        Task<int> CountAsync(Guid id);
    }
}
