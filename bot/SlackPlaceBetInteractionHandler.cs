using Akka.Actor;
using gamemaster.Extensions;
using gamemaster.Messages;

namespace gamemaster
{
    public class SlackPlaceBetInteractionHandler : SlackInteractionActionHandler
    {
        public override void Handle(string actionId, string userId,
            string responseUrl, string triggerId)
        {
            if (actionId.StartsWith("option_select"))
            {
                var parts = actionId.Split(':');
                var toteId = parts[1];
                var optionId = parts[2];
                UserContextsActor.Address.Tell(new PlaceBetSelectOptionMessage(userId, toteId, optionId));
            }
        }
    }
}