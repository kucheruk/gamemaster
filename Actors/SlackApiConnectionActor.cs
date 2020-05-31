using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackAPI;

namespace gamemaster.Actors
{
    public class SlackApiConnectionActor : ReceiveActor
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly ILogger<SlackApiConnectionActor> _logger;
        private readonly MessageRouter _router;
        private SlackTaskClient _client;

        public SlackApiConnectionActor(IOptions<SlackConfig> cfg, ILogger<SlackApiConnectionActor> logger,
            MessageRouter router)
        {
            _cfg = cfg;
            _logger = logger;
            _router = router;
            ReceiveAsync<MessageToChannel>(SendMessage);
        }


        private async Task SendMessage(MessageToChannel obj)
        {
            await _client.PostMessageAsync(obj.ChannelId, obj.Message);
        }


        protected override void PreStart()
        {
            base.PreStart();
            _router.RegisterSlackGateway(Self);
            _client = new SlackTaskClient(_cfg.Value.OauthToken);
            _client.EmitPresence(Presence.active);
        }
    }
}