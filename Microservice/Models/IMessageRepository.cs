using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Models
{
    public interface IMessageRepository : IDisposable
    {
        Task<ICollection<Message>> GetAllAsync(Expression<Func<Message, bool>> match, int offset = 0, int limit = 10);

        Task<ICollection<Message>> GetAllAsync(int offset = 0, int limit = 10);

        Task<ICollection<Message>> GetAllAsync(Expression<Func<Message, bool>> match);

        Task<Message> GetByIdAsync(Guid id);

        Task<int> AddAsync(Message message);

        Task<int> AddRangeAsync(IEnumerable<Message> messages);

        Task<int> DeleteByIdAsync(Guid id);

        Task<int> UpdateAsync(Message message);

        Task<int> UpdateRangeAsync(IEnumerable<Message> messages);

        Task<int> CountAsync(Guid id);
    }
}
