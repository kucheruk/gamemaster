using gamemaster.Extensions;

namespace gamemaster.CommandHandlers
{
    public class PlaceBetInteractionHandler
    {
        private readonly MessageRouter _router;

        public PlaceBetInteractionHandler(MessageRouter router)
        {
            _router = router;
        }

        public void HandleUserText(string user, string text)
        {
            _router.BetInfo(new PlaceBetMessage(user, text));
        }
    }
}