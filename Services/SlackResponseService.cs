using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace gamemaster.Services
{
    public class SlackResponseService
    {
        private readonly ILogger<SlackResponseService> _logger;
        private HttpClient Client { get; }

        public SlackResponseService(HttpClient client, ILogger<SlackResponseService> logger)
        {
            Client = client;
            _logger = logger;
        }

        public async Task ResponseWithText(string responseUrl, string txt,
            bool replaceOriginal = false, bool inChannel = false)
        {
            var rq = JsonConvert.SerializeObject(new
            {
                text = txt, 
                replace_original = replaceOriginal, 
                response_type = inChannel ? "in_channel" : "ephemeral"
            });
            _logger.LogInformation("Sending to slack! {Request}", rq);
            var response = await Client.PostAsync(responseUrl,
                new StringContent(rq));

            response.EnsureSuccessStatusCode();
        }
    }
}