using Akka.Actor;
using gamemaster.Extensions;

namespace gamemaster.CommandHandlers
{
    public class PlaceBetInteractionHandler
    {

        public void HandleUserText(string user, string text)
        {
            UserContextsActor.Address.Tell(new PlaceBetMessage(user, text));
        }
    }
}