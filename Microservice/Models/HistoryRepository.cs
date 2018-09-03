using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Microservice.Models
{
    public class HistoryRepository : IHistoryRepository
    {
        private TaskServiceContext _context = new TaskServiceContext();

        public async Task<ICollection<History>> GetAllAsync(int offset = 0, int limit = 10)
        {
            return await _context.Histories
                .Include(h => h.Status)
                .Include(h => h.Task)
                .OrderBy(h => h.CreatedAt)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<History> GetByIdAsync(Guid id)
        {
            return await _context.Histories.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<int> AddAsync(History history)
        {
            _context.Histories.Add(history);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> AddRangeAsync(IEnumerable<History> histories)
        {
            _context.Histories.AddRange(histories);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateAsync(History history)
        {
            _context.Entry(history).State = EntityState.Modified;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<History> histories)
        {
            if (histories == null || !histories.Any()) return 0;

            foreach (var history in histories)
            {
                _context.Entry(history).State = EntityState.Modified;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteByIdAsync(Guid id)
        {
            _context.Entry(new History() { Id = id }).State = EntityState.Deleted;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Guid id)
        {
            return await _context.Histories.CountAsync(s => s.Id == id);
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

        ~HistoryRepository()
        {
            Dispose(false);
        }
    }
}