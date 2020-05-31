using System.Linq;
using System.Text.RegularExpressions;
using gamemaster.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class EmissionRequestHandler
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly ILogger<EmissionRequestHandler> _logger;
        private readonly MessageRouter _router;
        private static readonly Regex IntRx = new Regex("^\\d+$", RegexOptions.Compiled);
        private static readonly Regex UserRx = new Regex("^<@([^|]+)\\|(.*?)>$");

        public EmissionRequestHandler(IOptions<SlackConfig> cfg, ILogger<EmissionRequestHandler> logger,
            MessageRouter router)
        {
            _cfg = cfg;
            _logger = logger;
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
                    var userId = FindUserId(parts);
                    if (userId.HasValue)
                    {
                        var currency = FindCurrency(parts, Constants.DefaultCurrency);
                        var amount = FindInteger(parts, 0);
                        if (amount > 0)
                        {
                            _router.LedgerEmit(userId.Value.id, currency, amount, user, responseUrl);
                            return (true, "Запрос на эмиссию монет прошёл успешно, обрабатываем");
                        }

                        return (false, $"Не смогли найти сумму выпускаемых средст в запросе");
                    }

                    return (false, "Не смогли найти пользователя, на которого вводятся средства");
                }

                return (false, "Неверный формат запроса. Пример: /emit @ek 300 :coin:");
            }

            return (false, "Эмиссия средств доступна только администратору. Обидно, да!");
        }
        
        
        private int FindInteger(string[] parts, int def)
        {
            var p = parts.FirstOrDefault(IntRx.IsMatch);
            if (p != null)
            {
                return int.Parse(p);
            }

            return def;
        }

        private string FindCurrency(string[] parts, string defaultCurrency)
        {
            return parts.FirstOrDefault(a => a.StartsWith(':') && a.EndsWith(':')) ?? defaultCurrency;
        }

        //<@U033GDN1S|kucheruk>
        private (string id, string name)? FindUserId(string[] parts)
        {
            var uid = parts.FirstOrDefault(UserRx.IsMatch);
            if (uid == null)
            {
                return null;
            }

            var p = UserRx.Match(uid);
            return (p.Groups[1].ToString(), p.Groups[2].ToString());
        }

        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }


    }
}