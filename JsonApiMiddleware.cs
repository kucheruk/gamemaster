using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using gamemaster.CommandHandlers;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class JsonApiMiddleware
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly EmissionRequestHandler _emissionHandler;
        private readonly ILogger<JsonApiMiddleware> _logger;
        private readonly MessageRouter _router;
        private readonly SlackRequestSignature _slackSignature;
        private readonly BalanceRequestHandler _balanceHandler;
        private readonly TossACoinHandler _tossHandler;

        public JsonApiMiddleware(RequestDelegate _,
            IOptions<SlackConfig> cfg,
            EmissionRequestHandler emissionHandler,
            MessageRouter router,
            SlackRequestSignature slackSignature,
            ILogger<JsonApiMiddleware> logger, 
            BalanceRequestHandler balanceHandler, 
            TossACoinHandler tossHandler)
        {
            _cfg = cfg;
            _emissionHandler = emissionHandler;
            _router = router;
            _slackSignature = slackSignature;
            _logger = logger;
            _balanceHandler = balanceHandler;
            _tossHandler = tossHandler;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var request = context.Request;
                request.EnableBuffering();
                if (request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestamp))
                {
                    if (request.Headers.TryGetValue("X-Slack-Signature", out var signature))
                    {
                        var bodyAsText = await ReadRequestAsString(request);
                        var keyValid = _slackSignature.Validate(bodyAsText, timestamp, signature);
                        if (keyValid)
                        {
                            _logger.LogInformation("{Body}", bodyAsText);
                            var mediaType = GetMediaType(request);
                            if (mediaType == "application/json")
                            {
                                var rq = JObject.Parse(bodyAsText);
                                if (rq["type"]?.ToString() == "url_verification")
                                {
                                    await ResponseChallenge(context.Response, rq["challenge"]?.ToString(),
                                        rq["token"]?.ToString());
                                    return;
                                }

                                var uri = request.Path;
                                if (rq["type"]?.ToString() == "event_callback")
                                {
                                    await HandleEvent(context.Response, rq);
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
                                    //channel_name=&
                                    MessageContext mctx =
                                        parts.TryGetValue("channel_name", out var ch) && ch == "directmessage"
                                            ? MessageContext.Direct
                                            : MessageContext.Group;
                                    var resp = HandleCommand(user, command, text, mctx, responseUrl);
                                    context.Response.StatusCode = 200;
                                    await context.Response.WriteAsync(resp.reason);
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
        
        private (bool success, string reason) HandleCommand(string user, string command,
            string text, MessageContext mctx, string responseUrl)
        {
            switch (command)
            {
                case "/emit":
                case "/semit":
                    return _emissionHandler.HandleEmission(user, text, responseUrl);
                case "/balance":
                case "/sbalance":
                    return _balanceHandler.HandleBalance(user, responseUrl, mctx);
                case "/toss":
                case "/stoss":
                    return _tossHandler.HandleToss(user, text, responseUrl);
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

        private Task HandleEvent(HttpResponse resp, JObject rq)
        {
            var @event = rq["event"];
            if (@event?["type"]?.ToString() == "message")
            {
                var botId = @event["bot_id"]?.ToString();
                var clientMsgId = @event["client_msg_id"]?.ToString();
                if (!string.IsNullOrEmpty(botId) && string.IsNullOrEmpty(clientMsgId))
                {
                    return Task.CompletedTask; // quick and dirty: ignore self (message loop)
                }

                var txt = @event["text"]?.ToString();
                var author = @event["user"]?.ToString();
                _router.ToSlackGateway(new MessageToChannel(author, $"echo {txt}"));
            }

            return Task.CompletedTask;
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