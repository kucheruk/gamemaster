using System.Linq;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Queries.Tote;

namespace gamemaster.CommandHandlers.Tote
{
    public class PromoListTextCommandHandler : ITextCommandHandler
    {
        private readonly PromoListQuery _list;

        public PromoListTextCommandHandler(PromoListQuery list)
        {
            _list = list;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Promo && text.StartsWith("list");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var list = await _list.ListPromosAsync();
            var response = list.Count > 0 ? string.Join("\n", list.Where(a => !a.Activated).Select(a => $"`{a.Code}` ={a.Amount}")) : "Нет активных промокодов";
            return (true, response);
        }
    }
}