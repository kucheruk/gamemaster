using Newtonsoft.Json.Linq;

namespace gamemaster.Slack
{
    public abstract class SlackBlock
    {
        public abstract JObject ToJson();
    }
}