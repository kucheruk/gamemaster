using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;
using SlackAPI;

namespace gamemaster.CommandHandlers.Ledger
{
    public class TossACoinHandler
    {
        private readonly SlackApiWrapper _slack;
        private readonly ILogger<TossACoinHandler> _logger;

        public TossACoinHandler(SlackApiWrapper slack,
            ILogger<TossACoinHandler> logger)
        {
            _slack = slack;
            _logger = logger;
        }


        public async Task<(bool success, string reason)> HandleTossAsync(string fromUser, string text,
            string responseUrl, MessageContext mctx)
        {
            TossRequestParams p = TossRequestParams.FromText(text);
            if (p == null)
            {
                return (false,
                    $"Не смогли найти пользователя, которому отправить монеток, {mctx.Type} {mctx.ChannelId}");

            }

            if (string.IsNullOrEmpty(p.Currency))
            {
                return (false, "Не смогли определить какие именно монетки переводить");
            }

            if (p.Amount <= 0)
            {
                return (false, "Не смогли найти в команде сколько монет переводим");
            }

            if (!string.IsNullOrEmpty(p.UserId))
            {
                return HandleTransferToSingleUser(fromUser, responseUrl, p);
            }


            if (mctx.Type == ChannelType.Group || mctx.Type == ChannelType.Channel)
            {
                return await HandleTransferToGroup(fromUser, responseUrl, mctx, p);
            }

            return (false, "И пользователь не указан, и команда не внутри канала... Как-то неочевидно, чего делать");
        }
        
        private async Task<(bool success, string reason)> HandleTransferToGroup(string fromUser, string responseUrl,
            MessageContext channel, TossRequestParams p)
        {
            var channelUsers = await _slack.GetChannelMembers(channel);
            var allUsers = await _slack.GetUserListAsync();
            channelUsers = FilterBots(channelUsers, allUsers);
            if (channelUsers == null)
            {
                return (false,
                    "Если хочешь, чтоб я раскидал монеты по пользователям закрытого канала - добавь туда этого бота");
            }

            foreach (var user in channelUsers)
            {
                _logger.LogInformation($"ttg, {user}");
            }

            if (channelUsers.Length > 1)
            {
                var msg = new GiveAwayMessage(fromUser, p.Currency, responseUrl, p.Amount, channelUsers, channel, p.Comment);
                LedgerActor.Address.Tell(msg);
                return (true, "Приказали гоблинам раскидать монетки всем пользователям канала...");
            }

            return (true, "Не очень-то понятно чего делать!");
        }

        private string[] FilterBots(string[] channelUsers, IDictionary<string, User> allUsers)
        {
            return channelUsers.Where(a => allUsers.TryGetValue(a, out var user) && !user.is_bot).ToArray();
        }

        private (bool success, string reason) HandleTransferToSingleUser(string fromUser, string responseUrl,
            TossRequestParams p)
        {
            LedgerActor.Address.Tell(new TossACoinMessage(fromUser, p.Currency, responseUrl, p.Amount,
                p.UserId, p.Comment));
            return (true, "Запрос на перевод отправлен гоблинам в банк, ожидай ответа");
        }
    }
}