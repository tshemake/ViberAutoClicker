using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Models
{
    public interface IHistoryRepository : IDisposable
    {
        Task<ICollection<History>> GetAllAsync(int offset = 0, int limit = 10);

        Task<History> GetByIdAsync(Guid id);

        Task<int> AddAsync(History history);

        Task<int> AddRangeAsync(IEnumerable<History> histories);

        Task<int> DeleteByIdAsync(Guid id);

        Task<int> UpdateAsync(History history);

        Task<int> UpdateRangeAsync(IEnumerable<History> histories);

        Task<int> CountAsync(Guid id);
    }
}
