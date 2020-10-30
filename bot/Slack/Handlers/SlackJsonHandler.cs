using System.Threading.Tasks;

namespace gamemaster.Slack.Handlers
{
    public abstract class SlackJsonHandler
    {
        public abstract Task<bool> Handle(SlackRequestContainer req);
    }
}