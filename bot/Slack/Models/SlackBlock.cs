using Newtonsoft.Json.Linq;

namespace gamemaster.Slack.Models
{
    public abstract class SlackBlock
    {
        public abstract JObject ToJson();
    }
}