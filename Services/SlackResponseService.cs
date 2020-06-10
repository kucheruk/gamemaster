using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SlackAPI;

namespace gamemaster.Services
{
    public class SlackResponseService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly ILogger<SlackResponseService> _logger;

        public SlackResponseService(HttpClient client, ILogger<SlackResponseService> logger)
        {
            Client = client;
            _logger = logger;
        }

        private HttpClient Client { get; }

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

        public async Task ResponseWithBlocks(string responseUrl, List<IBlock> blocks,
            bool inChannel)
        {
            var rq = JsonConvert.SerializeObject(new
            {
                blocks,
                response_type = inChannel ? "in_channel" : "ephemeral"
            }, _jsonSerializerSettings);
            _logger.LogInformation("Sending to slack! {Request}", rq);
            var response = await Client.PostAsync(responseUrl,
                new StringContent(rq, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();
        }
    }
}