using System;
using System.Collections.Generic;
using System.Linq;
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
            ReceiveAsync<GetChannelUsersRequestMessage>(GetUsers);
        }


        private async Task SendMessage(MessageToChannel obj)
        {
            await _client.PostMessageAsync(obj.ChannelId, obj.Message);
        }

        private async Task GetUsers(GetChannelUsersRequestMessage msg)
        {
            if (msg.Context.Type == ChannelType.Group)
            {
                var groups = await _client.GetGroupsListAsync();
                var group = groups.groups.FirstOrDefault(a => a.id == msg.Context.ChannelId);
                if (group != null)
                {
                    Sender.Tell(group.members);
                }
            }
            else if (msg.Context.Type == ChannelType.Channel)
            {
                var channels = await _client.GetChannelListAsync();
                var channel = channels.channels.FirstOrDefault(a => a.id == msg.Context.ChannelId);
                if (channel != null)
                {
                    Sender.Tell(channel.members);
                }
            }

            Sender.Tell(Array.Empty<string>());
        }
        
        private async Task GetAllUsers( )
        {
            var response = await _client.GetUserListAsync();
            if (response.ok)
            {
                Sender.Tell(response.members);
            }
            _logger.LogError("{Error} getting users from slack ", response.error);
            
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