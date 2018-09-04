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
using Microservice.ViewModel;

namespace Microservice.Controllers
{
    [Route("api/Tasks")]
    public class MessagesController : ApiController
    {
        private IMessageRepository _repository;
        private static readonly Guid pendingId = Guid.Parse("761CF6D7-7B24-48A1-9D98-64F7C4F99B25");
        private static readonly Guid activeId = Guid.Parse("8EE0B305-4D92-4438-89CE-DD0C8260451D");
        private static readonly Guid resumeId = Guid.Parse("D0074B8C-50BF-4631-90EE-9E6C869AD7BF");
        private static readonly Guid failureId = Guid.Parse("09BDBDEE-46EA-451A-8049-4D1390BE8B25");

        public MessagesController(IMessageRepository repository)
        {
            _repository = repository;
        }

        // GET: api/Tasks
        /// <summary>
        /// Получить существующие задачи
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<ICollection<Message>> Get(int offset = 0, int limit = 10)
        {
            return await _repository.GetAllAsync(offset, limit);
        }

        // PUT: api/Tasks
        /// <summary>
        /// Добавить задачи
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ResponseType(typeof(InfoTask))]
        public async Task<IHttpActionResult> Put(AddTask model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ICollection<Message> tasks = new List<Message>();
            foreach (var domain in model.Domains)
            {
                foreach (string phone in domain.Phones)
                {
                    Message task = new Message {
                        PhoneNumber = phone,
                        Content = domain.Message,
                        StatusId = pendingId
                    };
                    tasks.Add(task);
                }
            }

            try
            {
                await _repository.AddRangeAsync(tasks);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            InfoTask result = new InfoTask {
                Domains = tasks.GroupBy(msg => msg.Content,
                (key, group) => new InfoDomain {
                    Message = key,
                    Tasks = group.Select(msg => (Info)msg).ToList()
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/Tasks/Resume
        /// <summary>
        /// Перезапуск зависших задач
        /// </summary>
        /// <returns></returns>
        [Route("api/Tasks/Resume")]
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> Post()
        {
            DateTime earlier = DateTime.Now.AddDays(-1);
            var tasks = await _repository.GetAllAsync(m => m.StatusId == activeId && m.UpdatedAt <= earlier);

            foreach (var task in tasks)
            {
                task.StatusId = resumeId;
            };

            try
            {
                await _repository.UpdateRangeAsync(tasks);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Tasks/Take
        /// <summary>
        /// Получить задачи на выполнение
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        [Route("api/Tasks/Take")]
        [ResponseType(typeof(InfoTask))]
        public async Task<IHttpActionResult> Post(int limit = 10)
        {
            var tasks = await _repository.GetAllAsync(m => m.StatusId == pendingId
                || m.StatusId == resumeId
                || m.StatusId == failureId, limit: limit);

            foreach (var task in tasks)
            {
                task.StatusId = activeId;
            };

            try
            {
                await _repository.UpdateRangeAsync(tasks);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            InfoTask result = new InfoTask {
                Domains = tasks.GroupBy(msg => msg.Content,
                (key, group) => new InfoDomain {
                    Message = key,
                    Tasks = group.Select(msg => (Info)msg).ToList()
                }).ToList()
            };

            return Ok(result);
        }

        // POST: api/Tasks/Update
        /// <summary>
        /// Обновить статус задач
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("api/Tasks/Update")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Post(ResponeTask model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ICollection<Message> tasks = new List<Message>();
            foreach (var task in model.Tasks)
            {
                var message = await _repository.GetByIdAsync(task.Id);
                message.StatusId = task.StatusId;
                tasks.Add(message);
            }

            try
            {
                await _repository.UpdateRangeAsync(tasks);
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Tasks/5
        /// <summary>
        /// Удалить задачу
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [ResponseType(typeof(void))]
        private async Task<IHttpActionResult> Delete(Guid id)
        {
            if (!await MessageExistsAsync(id))
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

        private async Task<bool> MessageExistsAsync(Guid id)
        {
            return await _repository.CountAsync(id) > 0;
        }
    }
}