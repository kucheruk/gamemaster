using System.Threading.Tasks;
using gamemaster.Messages;

namespace gamemaster.CommandHandlers
{
    public class TossACoinHandler
    {
        private readonly MessageRouter _router;

        public TossACoinHandler(MessageRouter router)
        {
            _router = router;
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
                var amount = CommandsPartsParse.FindInteger(parts, 0);
                if (amount <= 0)
                {
                    return (false, "Не смогли найти в команде сколько монет переводим");
                }
    
                var userId = CommandsPartsParse.FindUserId(parts);
                if (userId.HasValue)
                {
                    return HandleTransferToSingleUser(fromUser, responseUrl, currency, amount, userId);
                }

                if (mctx.Type == ChannelType.Group)
                {
                    return await HandleTransferToGroup(fromUser, responseUrl, currency, amount, parts, mctx);
                }

                return (false, "Не смогли найти пользователя, которому отправить монеток");
            }

            return (false,
                "Неверный формат запроса. Формат: `/toss @кому сколько чего` Пример: `/toss @kucheruk 300 :coin:`");
        }

        private async Task<(bool success, string reason)> HandleTransferToGroup(string fromUser, string responseUrl,
            string currency, int amount, string[] parts, MessageContext channel)
        {
            var m = new GetChannelUsersRequestMessage(channel);
            var users = await _router.RpcToSlack<GetChannelUsersRequestMessage, string[]>(m);
            if (users == null && m.Context.Type == ChannelType.Group)
            {
                return (false,
                    "Если хочешь, чтоб я раскидал монеты по пользователям закрытого канала - добавь туда этого бота");
            }
            else
            {
                if (users != null && users.Length > 1)
                {
                    _router.LedgerGiveAway(new GiveAwayMessage(fromUser, currency, responseUrl, amount, users));
                    return (true, "Приказали гоблинам раскидать монетки всем пользователям канала...");
                }
            }
            return (true, "ok");
        }

        private (bool success, string reason) HandleTransferToSingleUser(string fromUser, string responseUrl,
            string currency, int amount, (string id, string name)? userId)
        {
                _router.LedgerToss(new TossACoinMessage(fromUser, currency, responseUrl, amount,
                    userId.Value.id));
                return (true, "Запрос на перевод отправлен гоблинам в банк, ожидай ответа");
        }
    }
}