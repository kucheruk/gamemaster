using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Config;
using gamemaster.Queries.Tote;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Tote
{
    public class PromoListFullTextCommandHandler : ITextCommandHandler
    {
        private readonly PromoListQuery _list;
        private readonly IOptions<SlackConfig> _cfg;

        public PromoListFullTextCommandHandler(PromoListQuery list, IOptions<SlackConfig> cfg)
        {
            _list = list;
            _cfg = cfg;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Promo && text.StartsWith("fulllist");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            if (_cfg.Value.Admins.Contains(cmd.UserId))
            {
                var list = await _list.ListPromosAsync(false);
                StringBuilder sb= new StringBuilder();
                foreach (var code in list.OrderBy(a => a.ActivatedOn))
                {
                    sb.AppendLine($"{code.Code} activated={code.Activated}, on {code.ActivatedOn}, <@{code.ToUserId}>");

                }
                return (true, sb.ToString());
            }

            return (false, "oops");
        }
    }
}