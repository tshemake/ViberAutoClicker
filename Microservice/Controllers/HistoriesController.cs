using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microservice.Models;

namespace Microservice.Controllers
{
    public class HistoriesController : ApiController
    {
        private IHistoryRepository _repository;

        public HistoriesController(IHistoryRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Histories
        /// <summary>
        /// Получить историю
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ICollection<History>> GetHistories(int offset = 0, int limit = 30)
        {
            return await _repository.GetAllAsync(limit);
        }

        // GET: api/Histories/5
        [ResponseType(typeof(History))]
        private async Task<IHttpActionResult> GetHistory(Guid id)
        {
            History history = await _repository.GetByIdAsync(id);
            if (history == null)
            {
                return NotFound();
            }

            return Ok(history);
        }

        // PUT: api/Histories/5
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> PutHistory(Guid id, History history)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != history.Id)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateAsync(history);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await HistoryExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Histories
        [ResponseType(typeof(History))]
        private async Task<IHttpActionResult> PostHistory(History history)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _repository.AddAsync(history);
            }
            catch (DbUpdateException)
            {
                if (await HistoryExistsAsync(history.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = history.Id }, history);
        }

        // DELETE: api/Histories/5
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> DeleteHistory(Guid id)
        {
            if (!await HistoryExistsAsync(id))
            {
                return NotFound();
            }

            await _repository.DeleteByIdAsync(id);

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task<bool> HistoryExistsAsync(Guid id)
        {
            return await _repository.CountAsync(id) > 0;
        }
    }
}