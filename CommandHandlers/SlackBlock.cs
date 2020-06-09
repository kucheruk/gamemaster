using Newtonsoft.Json.Linq;

namespace gamemaster.CommandHandlers
{
    public abstract class SlackBlock
    {
        public abstract JObject ToJson();
    }
}