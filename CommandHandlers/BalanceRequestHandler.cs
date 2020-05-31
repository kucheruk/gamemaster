using gamemaster.Messages;

namespace gamemaster.CommandHandlers
{
    public class BalanceRequestHandler
    {
        private readonly MessageRouter _router;

        public BalanceRequestHandler(MessageRouter router)
        {
            _router = router;
        }

        public (bool success, string reason) HandleBalance(string user, string text,
            string responseUrl)
        {
            _router.LedgerBalance(new GetBalanceMessage(user, responseUrl));
            return (true, "Гоблины проверяют твоё банковское хранилище...");
        }
    }
}