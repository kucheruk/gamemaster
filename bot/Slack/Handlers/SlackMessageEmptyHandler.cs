using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace gamemaster.Slack.Handlers
{
    public class SlackMessageEmptyHandler : SlackJsonHandler
    {
        public override Task<bool> Handle(JObject rq, HttpResponse response)
        {
            if (rq.ContainsKey("event"))
            {
                var e = rq["event"];
                if (e?["type"]?.ToString() == "message")
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }
}