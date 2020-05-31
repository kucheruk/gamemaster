using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace gamemaster
{
    public class SlackResponseService
    {
        private HttpClient Client { get; }

        public SlackResponseService(HttpClient client)
        {
            Client = client;
        }

        public async Task ResponseWithText(string responseUrl, string txt,
            bool replaceOriginal = false)
        {
            var response = await Client.PostAsync(responseUrl,
                new StringContent(JsonConvert.SerializeObject(new {text = txt, replace_original = replaceOriginal})));

            response.EnsureSuccessStatusCode();
        }
    }
}