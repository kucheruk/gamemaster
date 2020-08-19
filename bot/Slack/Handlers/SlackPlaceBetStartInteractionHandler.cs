using Akka.Actor;
using gamemaster.Extensions;

namespace gamemaster.Slack.Handlers
{
    public class SlackPlaceBetStartInteractionHandler : SlackInteractionActionHandler
    {
        public override void Handle(string actionId, string userId,
            string responseUrl, string triggerId)
        {
            if (actionId.StartsWith("start_bet"))
            {
                var parts = actionId.Split(':');
                var toteId = parts[1];
                UserContextsActor.Address.Tell(new PlaceBetStartMessage(userId, toteId, responseUrl, triggerId));
            }
        }
    }
}