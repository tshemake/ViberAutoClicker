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
    public class StatusesController : ApiController
    {
        private IStatusRepository _repository;

        public StatusesController(IStatusRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Status
        /// <summary>
        /// Получить статусы
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Status>> Get()
        {
            return await _repository.GetAllAsync();
        }

        // PUT: api/Status/5
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> Put(Guid id, Status status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != status.Id)
            {
                return BadRequest();
            }

            try
            {
                await _repository.UpdateAsync(status);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StatusExistsAsync(id))
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

        // POST: api/Status
        [ResponseType(typeof(Status))]
        private async Task<IHttpActionResult> Post(Status status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _repository.AddAsync(status);
            }
            catch (DbUpdateException)
            {
                if (await StatusExistsAsync(status.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = status.Id }, status);
        }

        // DELETE: api/Status/5
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> Delete(Guid id)
        {
            if (!await StatusExistsAsync(id))
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

        private async Task<bool> StatusExistsAsync(Guid id)
        {
            return await _repository.CountAsync(id) > 0;
        }
    }
}