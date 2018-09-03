using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Threading.Tasks;

namespace Microservice.Models
{
    public class StatusRepository : IStatusRepository
    {
        private TaskServiceContext _context = new TaskServiceContext();

        public async Task<ICollection<Status>> GetAllAsync()
        {
            return await _context.Statuses.ToListAsync();
        }

        public async Task<Status> GetByIdAsync(Guid id)
        {
            return await _context.Statuses.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> AddAsync(Status status)
        {
            _context.Statuses.Add(status);
            return await _context.SaveChangesAsync();
        }

        public Task<int> UpdateAsync(Status status)
        {
            _context.Entry(status).State = EntityState.Modified;

            return _context.SaveChangesAsync();
        }

        public async Task<int> DeleteByIdAsync(Guid id)
        {
            _context.Entry(new Status() { Id = id }).State = EntityState.Deleted;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(Guid id)
        {
            return await _context.Statuses.CountAsync(s => s.Id == id);
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
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }

            disposed = true;
        }

        ~StatusRepository()
        {
            Dispose(false);
        }
    }
}