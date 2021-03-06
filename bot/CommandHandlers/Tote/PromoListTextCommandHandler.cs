using System.Linq;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Config;
using gamemaster.Queries.Tote;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Tote
{
    public class PromoListTextCommandHandler : ITextCommandHandler
    {
        private readonly PromoListQuery _list;
        private readonly IOptions<SlackConfig> _cfg;

        public PromoListTextCommandHandler(PromoListQuery list, IOptions<SlackConfig> cfg)
        {
            _list = list;
            _cfg = cfg;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Promo && text.StartsWith("list");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            if (_cfg.Value.Admins.Contains(cmd.UserId))
            {
                var list = await _list.ListPromosAsync(true);
                var response = list.Count > 0 ? string.Join("\n", list.Where(a => !a.Activated).Select(a => $"`{a.Code}` ={a.Amount}")) : "Нет активных промокодов";
                return (true, response);
            }

            return (false, "oops");
        }
    }
}