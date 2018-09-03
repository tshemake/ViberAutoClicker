using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class API
    {
        public static string BaseAddress { get; set; }

        public static async Task<InfoTask> GetTasksAsync(HttpClient client, string uri, int limit = 10)
        {
            InfoTask task = null;
            Uri baseUri = new Uri(BaseAddress);
            HttpResponseMessage response = await client.PostAsync(new Uri(baseUri, "api/tasks/take?limit=" + limit), null);
            if (response.IsSuccessStatusCode)
            {
                task = await response.Content.ReadAsAsync<InfoTask>();
            }
            return task;
        }

        public static async Task<bool> UpdateTasksAsync(HttpClient client, string uri, ResponeTask responeTask)
        {
            Uri baseUri = new Uri(BaseAddress);
            HttpResponseMessage response = await client.PostAsJsonAsync(new Uri(baseUri, "api/tasks/update"), responeTask);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
