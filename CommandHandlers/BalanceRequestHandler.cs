using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class BalanceRequestHandler
    {
        private readonly MessageRouter _router;
        private readonly IOptions<SlackConfig> _cfg;

        public BalanceRequestHandler(MessageRouter router, IOptions<SlackConfig> cfg)
        {
            _router = router;
            _cfg = cfg;
        }

        public (bool success, string reason) HandleBalance(string user,
            string responseUrl, 
            MessageContext context)
        {
            _router.LedgerBalance(new GetBalanceMessage(user, IsAdmin(user), context, responseUrl));
            return (true, "Гоблины проверяют твоё банковское хранилище...");
        }
        
        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }
    }
}