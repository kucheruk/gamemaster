using gamemaster.Config;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class EmissionRequestHandler
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly MessageRouter _router;

        public EmissionRequestHandler(IOptions<SlackConfig> cfg, MessageRouter router)
        {
            _cfg = cfg;
            _router = router;
        }

        public (bool success, string reason) HandleEmission(string user, string text,
            string responseUrl)
        {
            if (IsAdmin(user))
            {
                var parts = text.Split(' ');
                if (parts.Length > 1)
                {
                    var userId = CommandsPartsParse.FindUserId(parts);
                    if (userId.HasValue)
                    {
                        var currency = CommandsPartsParse.FindCurrency(parts, Constants.DefaultCurrency);
                        var (_, amount) = CommandsPartsParse.FindDecimal(parts, 0);
                        if (amount > 0)
                        {
                            _router.LedgerEmit(userId.Value.id, currency, amount, user, responseUrl);
                            return (true, "Запрос на эмиссию монет прошёл успешно, обрабатываем");
                        }

                        return (false, "Не смогли найти сумму выпускаемых средст в запросе");
                    }

                    return (false, "Не смогли найти пользователя, на которого вводятся средства");
                }

                return (false, "Неверный формат запроса. Пример: /emit @ek 300 :coin:");
            }

            return (false, "Эмиссия средств доступна только администратору. Обидно, да!");
        }


        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }
    }
}