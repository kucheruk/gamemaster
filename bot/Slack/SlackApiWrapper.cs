using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SlackAPI;

namespace gamemaster.Slack
{
    public class SlackApiWrapper
    {
        private readonly SlackTaskClient _client;

        private readonly IDictionary<string, User> _emptyUsers = ImmutableDictionary<string, User>.Empty;
        private readonly ILogger<SlackApiWrapper> _logger;
        private readonly IHttpClientFactory _http;
        private readonly string _token;

        public SlackApiWrapper(IOptions<SlackConfig> cfg, ILogger<SlackApiWrapper> logger, IHttpClientFactory http)
        {
            _token = cfg.Value.OauthToken;
            _client = new SlackTaskClient(cfg.Value.OauthToken);
            _logger = logger;
            _http = http;
        }

        public async Task PostAsync(MessageToChannel msg)
        {
            await _client.PostMessageAsync(msg.ChannelId, msg.Message);
        }

        public async Task PostEphemeralAsync(EphemeralMessageToChannel msg)
        {
            await _client.PostEphemeralMessageAsync(msg.ChannelId, msg.Message, msg.UserId);
        }

        public async Task PostAsync(BlocksMessage msg)
        {
            await _client.PostMessageAsync(msg.ChannelId, text:string.Empty, blocks:msg.Blocks);
        }
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<string> UpdateMessage(string channelId, IBlock[] blocks, string ts)
        {
            var http = _http.CreateClient();
            var b = HttpUtility.UrlEncode(JsonConvert.SerializeObject(blocks, _jsonSerializerSettings));
            var uri =
                $"https://slack.com/api/chat.update?token={_token}&channel={channelId}&ts={ts}&blocks={b}&text=";
            var resp = await http.GetStringAsync(uri);
            
            //var resp =await _client.UpdateAsync(ts, channelId, string.Empty, blocks:blocks); //broken, as_user is deprecated by slack, but still sent by SlackApi lib
            return resp;
        }

        public async Task<PostMessageResponse> PostAsync(string channelId, IBlock[] blocks)
        {
            var response = await _client.PostMessageAsync(channelId, "", blocks: blocks);
            return response;
        }

        public async Task<string[]> GetChannelMembers(MessageContext msg)
        {
            var http = _http.CreateClient();
            var uri =
                $"https://slack.com/api/conversations.members?token={_token}&channel={msg.ChannelId}&limit={999}";
            var resp = await http.GetAsync(uri);
            if (!resp.IsSuccessStatusCode)
            {
                return Array.Empty<string>();
            }

            var stream = await resp.Content.ReadAsStreamAsync();
            var r = TryDeserializeFromStream<ConversationsMembersResponse>(stream);
            return r.Ok ? r.Members : Array.Empty<string>();
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
        
        
        private static T TryDeserializeFromStream<T>(Stream stream, bool isErrorStream = false)
        {
            if (isErrorStream)
            {
                try
                {
                    return DeserializeFromStream<T>(stream);
                }
                catch (JsonSerializationException)
                {
                    return default;
                }
                catch (JsonReaderException)
                {
                    return default;
                }
            }

            return DeserializeFromStream<T>(stream);
        }

        private static T DeserializeFromStream<T>(Stream stream)
        {
            if (stream.CanRead)
            {
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    return JsonSerializer.Deserialize<T>(jsonReader);
                }
            }
     
            return default(T);
        }
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();


        public async Task Dialog(string triggerId, SlackDialogModel toteDialog)
        {
            var http = _http.CreateClient();
            var uri =
                $"https://slack.com/api/views.open";
            
            var rq = JsonConvert.SerializeObject(new ViewsOpenRequest(triggerId, toteDialog));
            _logger.LogInformation("Request for create dialog: {Req}", rq);
            var request = new StringContent(rq, Encoding.UTF8, "application/json");
            var httpreq = new HttpRequestMessage(HttpMethod.Post, uri);
            httpreq.Headers.Add("Authorization", $"Bearer {_token}");
            httpreq.Content = request;
            var resp = await http.SendAsync(httpreq);
            var s = await resp.Content.ReadAsStringAsync();
            _logger.LogInformation("Response for create dialog: {Resp}", s);
            if (!resp.IsSuccessStatusCode)
            {
                
                return;
            }
        }
    }

    public class ConversationsMembersResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }
        
        [JsonProperty("members")]
        public string[] Members { get; set; }
    }
}