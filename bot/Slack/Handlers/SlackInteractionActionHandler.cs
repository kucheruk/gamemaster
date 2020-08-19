namespace gamemaster.Slack.Handlers
{
    public abstract class SlackInteractionActionHandler
    {
        public abstract void Handle(string actionId, string userId,
            string responseUrl, string triggerId);
    }
}