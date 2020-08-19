using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using gamemaster.CommandHandlers.Ledger;
using gamemaster.CommandHandlers.Tote;
using gamemaster.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace gamemaster
{
    public class SlackCommandFormHandler : SlackFormHandler
    {
        private readonly BalanceRequestHandler _balanceHandler;
        private readonly EmissionRequestHandler _emissionHandler;
        private readonly TossACoinHandler _tossHandler;
        private readonly ToteRequestHandler _toteHandler;
        private readonly ILogger<SlackCommandFormHandler> _logger;

        public SlackCommandFormHandler(BalanceRequestHandler balanceHandler, 
            EmissionRequestHandler emissionHandler,
            TossACoinHandler tossHandler, 
            ToteRequestHandler toteHandler,
            ILogger<SlackCommandFormHandler> logger)
        {
            _balanceHandler = balanceHandler;
            _emissionHandler = emissionHandler;
            _tossHandler = tossHandler;
            _toteHandler = toteHandler;
            _logger = logger;
        }

        public override async Task<bool> Handle(Dictionary<string, string> form, HttpResponse response)
        {  if (form.TryGetValue("command", out var command) &&
               form.TryGetValue("user_id", out var user) &&
               form.TryGetValue("text", out var text) &&
               form.TryGetValue("response_url", out var responseUrl))
            {
                var mc = GetMessageContext(form);
                var resp = await HandleCommandAsync(user, command, text, mc, responseUrl);
                response.StatusCode = 200;
                await response.WriteAsync(resp.reason);
                return true;
            }

            return false;
        }
        
        private async Task<(bool success, string reason)> HandleCommandAsync(string user, string command,
            string text, MessageContext mctx,
            string responseUrl)
        {
            switch (command)
            {
                case "/emit":
                    return _emissionHandler.HandleEmission(user, text, responseUrl);

                case "/tote":
                    return await _toteHandler.HandleToteAsync(user, text, mctx, responseUrl);
                case "/balance":
                    return _balanceHandler.HandleBalance(user, responseUrl, mctx);
                case "/toss":
                    return await _tossHandler.HandleTossAsync(user, text, responseUrl, mctx);
                default:
                {
                    _logger.LogError("Unknown command {Command} from {User} with {Text}", command, user, text);
                    return (false, $"Не опознанная команда {command}");
                }
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


    }
}