using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Config;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Tote
{
    public class PromoAddTextCommandHandler : ITextCommandHandler
    {
        private readonly IOptions<AppConfig> _app;
        private readonly PromoAddCommand _add;

        public PromoAddTextCommandHandler(IOptions<AppConfig> app, PromoAddCommand add)
        {
            _app = app;
            _add = add;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Promo && text.StartsWith("add");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var parts = cmd.Text.Split(' ');
            var (_, amount) = CommandsPartsParse.FindSignedDecimal(parts, 0);
            var currency = _app.Value.LimitToDefaultCurrency ? _app.Value.DefaultCurrency : CommandsPartsParse.FindCurrency(parts, _app.Value.DefaultCurrency);
            if (amount != 0)
            {
                var code = await _add.AddPromoAsync(cmd.UserId, amount, currency);
                return (true, $"Новый промокод на {amount}{currency}: {code.Code}");
            }

            return (false, "Необходимо указать положительное или отрицательное количество монет для промокода");
        }
    }
}