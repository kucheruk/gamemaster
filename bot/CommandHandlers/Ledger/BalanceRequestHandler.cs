using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers.Ledger
{
    public class BalanceRequestHandler
    {
        private readonly IOptions<SlackConfig> _cfg;

        public BalanceRequestHandler(IOptions<SlackConfig> cfg)
        {
            _cfg = cfg;
        }

        public (bool success, string reason) HandleBalance(string user,
            string responseUrl,
            MessageContext context)
        {
            LedgerActor.Address.Tell(new GetBalanceMessage(user, IsAdmin(user), context, responseUrl));
            return (true, "Гоблины проверяют твоё банковское хранилище...");
        }

        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }
    }
}