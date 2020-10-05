using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Commands;
using gamemaster.Config;
using gamemaster.Messages;
using gamemaster.Queries.Tote;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Tote
{
    public class PromoRequestHandler
    {
        private readonly IEnumerable<ITextCommandHandler> _commands;
        private readonly IOptions<SlackConfig> _slackCfg;
        private readonly PromoCodeFindQuery _find;
        private readonly PromoActivateCommand _activate;
        private readonly ILogger<PromoRequestHandler> _logger;

        public PromoRequestHandler(IEnumerable<ITextCommandHandler> commands, IOptions<SlackConfig> slackCfg,
            PromoCodeFindQuery find, PromoActivateCommand activate,
            ILogger<PromoRequestHandler> logger)
        {
            _commands = commands;
            _slackCfg = slackCfg;
            _find = find;
            _activate = activate;
            _logger = logger;
        }


        public async Task<(bool success, string reason)> TryEnterPromoAsync(string user, string text,
            MessageContext ctx, string responseUrl)
        {
            _logger.LogInformation("Request to activate promo from {user} {text}?");
            var code = await _find.FindPromoAsync(text.Trim());
            if (code == null)
            {
                return (false, null);
            }

            _logger.LogInformation("Got Promocode for {user} ({Code}, {Activated}", user, code.Code, code.Activated);
            if (code.Activated)
            {
                return (false, $"Код уже активировал кто-то более шустрый (<@{code.ToUserId}>)");
            }

            var activate = await _activate.ActivatePromoAsync(user, code.Code);
            if (activate)
            {
                LedgerActor.Address.Tell(new PromoTransferMessage
                {
                    Amount = code.Amount,
                    Currency = code.Currency,
                    FromUser = code.FromUserId,
                    ToUser = user,
                    Code = code.Code,
                    ResponseUrl = responseUrl
                }, ActorRefs.Nobody);
            }
            else
            {
                return (false, "Что-то пошло чудовищно не так");
            }

            return (false, null);
        }

        public async Task<(bool success, string reason)> HandlePromoAsync(string user, string text,
            MessageContext context, string responseUrl)
        {
            if (_slackCfg.Value.Admins.Contains(user))
            {
                foreach (var command in _commands)
                {
                    if (command.Match(TextCommandFamily.Promo, text))
                    {
                        return await command.Process(new SlackTextCommand(user, context, responseUrl, text));
                    }
                }

                return (true, "Какая-то очень странная и подозрительная команда!");
            }

            return (false, "Только для админов, сорян!");
        }
    }
}