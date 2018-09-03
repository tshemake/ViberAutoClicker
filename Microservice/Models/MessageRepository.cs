using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace Microservice.Models
{
    public class MessageRepository : IMessageRepository
    {
        private TaskServiceContext _context = new TaskServiceContext();

        public async Task<ICollection<Message>> GetAllAsync(Expression<Func<Message, bool>> match, int offset = 0, int limit = 10)
        {
            return await _context.Messages
                .Where(match)
                .Include(m => m.Status)
                .OrderBy(m => m.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ICollection<Message>> GetAllAsync(int offset = 0, int limit = 10)
        {
            return await _context.Messages
                .Include(m => m.Status)
                .OrderBy(m => m.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ICollection<Message>> GetAllAsync(Expression<Func<Message, bool>> match)
        {
            return await _context.Messages
                .Where(match)
                .Include(m => m.Status)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message> GetByIdAsync(Guid id)
        {
            return await _context.Messages.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> AddAsync(Message message)
        {
            _context.Messages.Add(message);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> AddRangeAsync(IEnumerable<Message> messages)
        {
            _context.Messages.AddRange(messages);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(Message message)
        {
            _context.Entry(message).State = EntityState.Modified;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<Message> messages)
        {
            if (messages == null || !messages.Any()) return 0;

            foreach (var message in messages)
            {
                _context.Entry(message).State = EntityState.Modified;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteByIdAsync(Guid id)
        {
            _context.Entry(new Message() { Id = id }).State = EntityState.Deleted;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Guid id)
        {
            return await _context.Messages.CountAsync(s => s.Id == id);
        }

        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
            }

            disposed = true;
        }

        ~MessageRepository()
        {
            Dispose(false);
        }
    }
}