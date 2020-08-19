using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Messages;

namespace gamemaster.Slack.Handlers
{
    public class SlackFinishToteInteractionHandler : SlackInteractionActionHandler
    {
        public override void Handle(string actionId, string userId,
            string responseUrl, string triggerId)
        {
            if (actionId.StartsWith("finish_tote"))
            {
                var parts = actionId.Split(':');
                var toteId = parts[1];
                var optionId = parts[2];
                TotesActor.Address.Tell(new ToteFinishedMessage(parts[1], parts[2], userId));
            }
        }
    }
}