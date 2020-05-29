using System.Threading;
using Akka.Actor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackAPI;
using SlackAPI.WebSocketMessages;

namespace gamemaster
{
    public class SlackApiConnectionActor : ReceiveActor
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly ILogger<SlackApiConnectionActor> _logger;
        private SlackSocketClient _client;

        public SlackApiConnectionActor(IOptions<SlackConfig> cfg, ILogger<SlackApiConnectionActor> logger)
        {
            _cfg = cfg;
            _logger = logger;
            Receive<SlackConnectedMessage>(_ => Become(Connected));
        }

        private void Connected()
        {
            Receive<NewMessage>(HandleNewMessage);
            _logger.LogInformation("Connected to slack!");
        }

        private void HandleNewMessage(NewMessage obj)
        {
            _logger.LogInformation("New message from slack {Message}", obj);
        }

        protected override void PreStart()
        {
            base.PreStart();
            _client = new SlackSocketClient(_cfg.Value.ClientSecret);
            _client.Connect(connected =>
            {
                Self.Tell(SlackConnectedMessage.Instance);
            }, () =>{
            });
            _client.OnMessageReceived += message =>
            {
                Self.Tell(message);
            };
        }
    }
}