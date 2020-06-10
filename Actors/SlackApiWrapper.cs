using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlackAPI;

namespace gamemaster.Actors
{
    public class SlackApiWrapper
    {
        private readonly SlackTaskClient _client;

        private readonly IDictionary<string, User> _emptyUsers = ImmutableDictionary<string, User>.Empty;
        private readonly ILogger<SlackApiWrapper> _logger;

        public SlackApiWrapper(IOptions<SlackConfig> cfg, ILogger<SlackApiWrapper> logger)
        {
            _client = new SlackTaskClient(cfg.Value.OauthToken);
            _logger = logger;
        }

        public async Task PostAsync(MessageToChannel msg)
        {
            await _client.PostMessageAsync(msg.ChannelId, msg.Message);
        }

        public async Task PostAsync(string channelId, IBlock[] blocks)
        {
            await _client.PostMessageAsync(channelId, "", blocks: blocks);
        }

        public async Task<string[]> GetChannelMembers(MessageContext msg)
        {
            if (msg.Type == ChannelType.Group)
            {
                var groups = await _client.GetGroupsListAsync();
                var group = groups.groups.FirstOrDefault(a => a.id == msg.ChannelId);
                if (group != null)
                {
                    return group.members;
                }
            }
            else if (msg.Type == ChannelType.Channel)
            {
                var channels = await _client.GetChannelListAsync();
                var channel = channels.channels.FirstOrDefault(a => a.id == msg.ChannelId);
                if (channel != null)
                {
                    return channel.members;
                }
            }

            return Array.Empty<string>();
        }

        public async Task<IDictionary<string, User>> GetUserListAsync()
        {
            var response = await _client.GetUserListAsync();

            if (response.ok)
            {
                return response.members.ToDictionary(a => a.id, a => a);
            }

            _logger.LogError("{Error} getting users from slack ", response.error);
            return _emptyUsers;
        }

        public void IAmOnline()
        {
            _client.EmitPresence(Presence.active);
        }
    }
}