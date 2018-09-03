using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Client.Models;

namespace Client
{
    public class API
    {
        public static string BaseAddress { get; set; }

        private static HttpClient _client = new HttpClient();

        public static async Task<InfoTask> GetTasksAsync(int limit = 10)
        {
            InfoTask task = null;
            Uri baseUri = new Uri(BaseAddress);
            HttpResponseMessage response = await _client.PostAsync(new Uri(baseUri, "api/tasks/take?limit=" + limit), null);
            if (response.IsSuccessStatusCode)
            {
                task = await response.Content.ReadAsAsync<InfoTask>();
            }
            return task;
        }

        public static async Task<bool> UpdateTasksAsync(ResponeTask responeTask)
        {
            Uri baseUri = new Uri(BaseAddress);
            HttpResponseMessage response = await _client.PostAsJsonAsync(new Uri(baseUri, "api/tasks/update"), responeTask);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
