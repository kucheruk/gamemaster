using System.Threading.Tasks;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace gamemaster.Slack.Handlers
{
    public class SlackMessageEmptyHandler : SlackJsonHandler
    {
        private readonly ILogger<SlackMessageEmptyHandler> _logger;
        private readonly PromoRequestHandler _promo;

        public SlackMessageEmptyHandler(ILogger<SlackMessageEmptyHandler> logger, PromoRequestHandler promo)
        {
            _logger = logger;
            _promo = promo;
        }

        public override async Task<bool> Handle(JObject rq, HttpResponse response)
        {
            if (rq.ContainsKey("event"))
            {
                var e = rq["event"];
                if (e?["type"]?.ToString() != "message")
                {
                    return false;
                }

                var msg = e["text"]?.ToString();
                var user = e["user"]?.ToString();
                var channel = e["channel"]?.ToString();
                var channelType = e["channel_type"]?.ToString();
                _logger.LogInformation("{Msg} {From} {Channel}", msg, user, channel);
                var res = await _promo.TryEnterPromoAsync(user, msg, new MessageContext()
                {
                    ChannelId = channel,
                    Type = ChannelType.Direct
                }, null);
                _logger.LogInformation(e.ToString());
                return true;
            }

            return false;
        }
    }
}