using System.Text.RegularExpressions;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;

namespace gamemaster.CommandHandlers
{
    public class TossACoinHandler
    {
        private readonly MessageRouter _router;
        private readonly ILogger<TossACoinHandler> _logger;
        private static readonly Regex IntRx = new Regex("^\\d+$", RegexOptions.Compiled);
        private static readonly Regex UserRx = new Regex("^<@([^|]+)\\|(.*?)>$");

        public TossACoinHandler(MessageRouter router, ILogger<TossACoinHandler> logger)
        {
            _router = router;
            _logger = logger;
        }


        public (bool success, string reason) HandleToss(string fromUser, string text,
            string responseUrl)
        {
            
            var parts = text.Split(' ');
            if (parts.Length > 1)
            {
                var userId = CommandsPartsParse.FindUserId(parts);
                if (userId.HasValue)
                {
                    var currency = CommandsPartsParse.FindCurrency(parts, Constants.DefaultCurrency);
                    var amount = CommandsPartsParse.FindInteger(parts, 0);
                    if (amount > 0)
                    {
                        _router.LedgerToss(new TossACoinMessage(fromUser, currency, responseUrl, amount, userId.Value.id));
                        return (true, "Запрос на перевод отправлен гоблинам в банк, ожидай ответа");
                    }

                    return (false, $"Не смогли найти сумму средст для перевода в запросе");
                }

                return (false, "Не смогли найти пользователя, которому отправить монеток");
            }

            return (false, "Неверный формат запроса. Формат: `/toss @кому сколько чего` Пример: `/toss @kucheruk 300 :coin:`");
        }
    }
}