using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class SlackEventCallbackHandler : SlackJsonHandler
    {
        public override async Task<bool> Handle(JObject rq, HttpResponse response)
        {
            if (rq["type"]?.ToString() == "event_callback")
            {
                await HandleEventAsync(response, rq);
                return true;
            }

            return false;
        }
        
        private async Task HandleEventAsync(HttpResponse resp, JObject rq)
        {
            var @event = rq["event"];
            if (@event?["type"]?.ToString() == "message")
            {
                var botId = @event["bot_id"]?.ToString();
                var clientMsgId = @event["client_msg_id"]?.ToString();
                if (!string.IsNullOrEmpty(botId) && string.IsNullOrEmpty(clientMsgId))
                {
                    return; // quick and dirty: ignore self (message loop)
                }

                var txt = @event["text"]?.ToString();
                var author = @event["user"]?.ToString();
                // TODO: response to simple messages?
            }
        }


    }
}