using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.CommandHandlers.Ledger;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Config;
using gamemaster.Extensions;
using gamemaster.Messages;
using gamemaster.Slack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class JsonApiMiddleware
    {
        private readonly BalanceRequestHandler _balanceHandler;
        private readonly IOptions<SlackConfig> _cfg;
        private readonly EmissionRequestHandler _emissionHandler;
        private readonly ILogger<JsonApiMiddleware> _logger;
        private readonly SlackRequestSignature _slackSignature;
        private readonly TossACoinHandler _tossHandler;
        private readonly ToteRequestHandler _toteHandler;
        private readonly SlackApiWrapper _slack;

        public JsonApiMiddleware(RequestDelegate _,
            IOptions<SlackConfig> cfg,
            EmissionRequestHandler emissionHandler,
            SlackRequestSignature slackSignature,
            ILogger<JsonApiMiddleware> logger,
            BalanceRequestHandler balanceHandler,
            TossACoinHandler tossHandler,
            ToteRequestHandler toteHandler,
            SlackApiWrapper slack)
        {
            _cfg = cfg;
            _emissionHandler = emissionHandler;
            _slackSignature = slackSignature;
            _logger = logger;
            _balanceHandler = balanceHandler;
            _tossHandler = tossHandler;
            _toteHandler = toteHandler;
            _slack = slack;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var request = context.Request;
                request.EnableBuffering();
                var bodyAsText = await ReadRequestAsString(request);
                var mediaType = GetMediaType(request);
                _logger.LogInformation("mime={MediaType}, body={Body}", mediaType, bodyAsText);
                if (request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestamp))
                {
                    if (request.Headers.TryGetValue("X-Slack-Signature", out var signature))
                    {
                        var keyValid = _slackSignature.Validate(bodyAsText, timestamp, signature);
                        if (keyValid)
                        {
                            if (mediaType == "application/json")
                            {
                                var rq = JObject.Parse(bodyAsText);
                                if (rq["type"]?.ToString() == "url_verification")
                                {
                                    await ResponseChallenge(context.Response, rq["challenge"]?.ToString(),
                                        rq["token"]?.ToString());
                                    return;
                                }

                                if (rq.ContainsKey("event"))
                                {
                                    var e = rq["event"];
                                    if (e?["type"]?.ToString() == "message")
                                    {
                                        return;
                                    }
                                }

                                var uri = request.Path;
                                if (rq["type"]?.ToString() == "event_callback")
                                {
                                    await HandleEventAsync(context.Response, rq);
                                }
                            }
                            else if (mediaType == "application/x-www-form-urlencoded")
                            {
                                var parts = bodyAsText.Split("&").Select(a => a.Split("="))
                                    .ToDictionary(a => a[0], a => HttpUtility.UrlDecode(a[1]));
                                if (parts.TryGetValue("command", out var command) &&
                                    parts.TryGetValue("user_id", out var user) &&
                                    parts.TryGetValue("text", out var text) &&
                                    parts.TryGetValue("response_url", out var responseUrl))
                                {
                                    var mc = GetMessageContext(parts);
                                    var resp = await HandleCommandAsync(user, command, text, mc, responseUrl);
                                    context.Response.StatusCode = 200;
                                    await context.Response.WriteAsync(resp.reason);
                                }
                                else if (parts.TryGetValue("payload", out var payload))
                                {
                                    var sw = new Stopwatch();
                                    sw.Start();
                                    var pl = DeserializePayload(payload);
                                    _logger.LogInformation("Got pl={Payload}", JsonConvert.SerializeObject(pl));
                                    HandleInteraction(pl);
                                    context.Response.StatusCode = 200;
                                    await context.Response.CompleteAsync();
                                    sw.Stop();
                                    _logger.LogWarning("Interaction handled: {ДС}", sw.ElapsedMilliseconds);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling request");
            }
        }

        private void HandleInteraction(SlackInteractionPayload pl)
        {
            if (pl?.Actions?.Count > 0)
            {
                foreach (var action in pl.Actions)
                {
                    HandleInteractionAction(action, pl.User.Id, pl.ResponseUrl, pl.TriggerId);
                }
            }
            else
            {
                var values = pl?.View?.State?.Values;
                if (values != null)
                {
                    var v = values;
                    var vals = v.SelectMany(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
                    if (vals.TryGetValue("bet_option", out var option) && vals.TryGetValue("bet_amount", out var amount))
                    {
                        var cb = pl.View.CallbackId;
                        if (!string.IsNullOrEmpty(cb))
                        {
                            var parts = cb.Split(':');
                            if (parts.Length > 1)
                            {
                                var toteId = parts[1];
                                var userId = pl.User.Id;
                                var optId = option.SelectedOption.Value;
                                var am =  decimal.Parse(amount.Value).Trim();
                                TotesActor.Address.Tell(new TotePlaceBetMessage(userId, toteId, optId, am, pl?.Channel?.Id));
                            }
                        }
                    }
                }
            }
        }

        private void HandleInteractionAction(SlackInteractionAction action, string userId,
            string plResponseUrl, string triggerId)
        {
            if (action.ActionId.StartsWith("finish_tote"))
            {
                var parts = action.ActionId.Split(':');
                HandleFinishTote(parts[1], parts[2], userId);
            }

            if (action.ActionId.StartsWith("start_bet"))
            {
                var parts = action.ActionId.Split(':');
                HandleStartBet(parts[1], userId, plResponseUrl, triggerId);
            }

            if (action.ActionId.StartsWith("option_select"))
            {
                var parts = action.ActionId.Split(':');
                HandleSelectNumber(parts[1], parts[2], userId);
            }
        }

        private void HandleStartBet(string toteId, string userId,
            string plResponseUrl, string triggerId)
        {
            UserContextsActor.Address.Tell(new PlaceBetStartMessage(userId, toteId, plResponseUrl, triggerId));
        }

        private void HandleSelectNumber(string toteId, string optionId,
            string userId)
        {
            UserContextsActor.Address.Tell(new PlaceBetSelectOptionMessage(userId, toteId, optionId));
        }

        private void HandleFinishTote(string toteId, string optionId,
            string userId)
        {
            TotesActor.Address.Tell(new ToteFinishedMessage(toteId, optionId, userId));
        }

        private SlackInteractionPayload DeserializePayload(string payload)
        {
            return JsonConvert.DeserializeObject<SlackInteractionPayload>(payload);
        }

        private async Task<(bool success, string reason)> HandleCommandAsync(string user, string command,
            string text, MessageContext mctx,
            string responseUrl)
        {
            switch (command)
            {
                case "/emit":
                case "/semit":
                    return _emissionHandler.HandleEmission(user, text, responseUrl);

                case "/tote":
                case "/stote":
                    return await _toteHandler.HandleToteAsync(user, text, mctx, responseUrl);
                case "/balance":
                case "/sbalance":
                    return _balanceHandler.HandleBalance(user, responseUrl, mctx);
                case "/toss":
                case "/stoss":
                    return await _tossHandler.HandleTossAsync(user, text, responseUrl, mctx);
                default:
                {
                    _logger.LogError("Unknown command {Command} from {User} with {Text}", command, user, text);
                    return (false, $"Не опознанная команда {command}");
                }
            }
        }

        private static StringSegment GetMediaType(HttpRequest request)
        {
            return request.GetTypedHeaders().ContentType.MediaType;
        }

        private static async Task<string> ReadRequestAsString(HttpRequest request)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];
            await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var bodyAsText = Encoding.UTF8.GetString(buffer);
            return bodyAsText;
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
                await _slack.PostAsync(new MessageToChannel(author, $"echo {txt}"));
            }
        }

        private MessageContext GetMessageContext(Dictionary<string, string> parts)
        {
            if (parts.TryGetValue("channel_id", out var id) && parts.TryGetValue("channel_name", out var name))
            {
                var ct = name switch
                {
                    "privategroup" => ChannelType.Group,
                    "directmessage" => ChannelType.Direct,
                    _ => ChannelType.Channel
                };
                return new MessageContext(ct, id);
            }

            throw new InvalidDataException("can't determine channel type and id");
        }

        private async Task ResponseChallenge(HttpResponse resp, string challenge,
            string token)
        {
            if (_cfg.Value.VerificationToken == token)
            {
                resp.StatusCode = 200;
                await resp.StartAsync();
                await resp.WriteAsync(challenge);
            }
        }
    }
}