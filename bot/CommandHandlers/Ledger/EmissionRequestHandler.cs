using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Ledger
{
    public class EmissionRequestHandler
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly IOptions<AppConfig> _app;

        public EmissionRequestHandler(IOptions<SlackConfig> cfg, IOptions<AppConfig> app)
        {
            _cfg = cfg;
            _app = app;
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
                        var currency = CommandsPartsParse.FindCurrency(parts, _app.Value.DefaultCurrency);
                        var (_, amount) = CommandsPartsParse.FindDecimal(parts, 0);
                        if (amount > 0)
                        {
                            LedgerActor.Address.Tell(new EmitMessage(userId.Value.id, currency, amount, user, responseUrl));
                            return (true, "Запрос на эмиссию монет прошёл успешно, обрабатываем");
                        }

                        return (false, "Не смогли найти сумму выпускаемых средст в запросе");
                    }

                    return (false, "Не смогли найти пользователя, на которого вводятся средства");
                }

                return (false, TossExample());
            }

            return (false, "Эмиссия средств доступна только администратору. Обидно, да!");
        }

        private string TossExample()
        {
            var e = "Неверный формат запроса. Пример: /emit @ek 300";
            if (_app.Value.LimitToDefaultCurrency)
            {
                return e;
            }

            return e + " " + _app.Value.DefaultCurrency;
        }


        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }
    }
}