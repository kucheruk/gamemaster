using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Actors;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;
using SlackAPI;

namespace gamemaster.CommandHandlers
{
    public class TossACoinHandler
    {
        private readonly MessageRouter _router;
        private readonly SlackApiWrapper _slack;
        private readonly ILogger<TossACoinHandler> _logger;

        public TossACoinHandler(MessageRouter router, SlackApiWrapper slack,
            ILogger<TossACoinHandler> logger)
        {
            _router = router;
            _slack = slack;
            _logger = logger;
        }


        public async Task<(bool success, string reason)> HandleTossAsync(string fromUser, string text,
            string responseUrl, MessageContext mctx)
        {
            var parts = text.Split(' ');
            if (parts.Length > 1)
            {
                var currency = CommandsPartsParse.FindCurrency(parts, Constants.DefaultCurrency);
                if (string.IsNullOrEmpty(currency))
                {
                    return (false, "Не смогли определить какие именно монетки переводить");
                }

                var (amountstr, amount) = CommandsPartsParse.FindDecimal(parts, 0);
                if (amount <= 0)
                {
                    return (false, "Не смогли найти в команде сколько монет переводим");
                }

                var rest = text.Remove(text.IndexOf(currency, StringComparison.OrdinalIgnoreCase), currency.Length);
                rest = rest.Remove(rest.IndexOf(amountstr, StringComparison.OrdinalIgnoreCase), amountstr.Length);

                var userId = CommandsPartsParse.FindUserId(parts);
                if (userId.HasValue)
                {
                    rest = rest.Remove(rest.IndexOf(userId.Value.part, StringComparison.OrdinalIgnoreCase),
                        userId.Value.part.Length);

                    return HandleTransferToSingleUser(fromUser, responseUrl, currency, amount, userId, rest.Trim());
                }


                if (mctx.Type == ChannelType.Group || mctx.Type == ChannelType.Channel)
                {
                    return await HandleTransferToGroup(fromUser, responseUrl, currency, amount, mctx, rest.Trim());
                }

                return (false, $"Не смогли найти пользователя, которому отправить монеток, {mctx.Type} {mctx.ChannelId}");
            }

            return (false,
                "Неверный формат запроса. Формат: `/toss @кому сколько чего` Пример: `/toss @kucheruk 300 :coin:`");
        }

        private async Task<(bool success, string reason)> HandleTransferToGroup(string fromUser, string responseUrl,
            string currency, decimal amount,
            MessageContext channel, string comment)
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
                _router.LedgerGiveAway(new GiveAwayMessage(fromUser, currency, responseUrl, amount, channelUsers,
                    channel,
                    comment));
                return (true, "Приказали гоблинам раскидать монетки всем пользователям канала...");
            }

            return (true, "Не очень-то понятно чего делать!");
        }

        private string[] FilterBots(string[] channelUsers, IDictionary<string, User> allUsers)
        {
            return channelUsers.Where(a => allUsers.TryGetValue(a, out var user) && !user.is_bot).ToArray();
        }

        private (bool success, string reason) HandleTransferToSingleUser(string fromUser, string responseUrl,
            string currency, decimal amount,
            (string id, string name)? userId, string comment)
        {
            _router.LedgerToss(new TossACoinMessage(fromUser, currency, responseUrl, amount,
                userId.Value.id, comment));
            return (true, "Запрос на перевод отправлен гоблинам в банк, ожидай ответа");
        }
    }
}