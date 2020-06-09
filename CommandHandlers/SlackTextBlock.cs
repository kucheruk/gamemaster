using Newtonsoft.Json.Linq;

namespace gamemaster.CommandHandlers
{
    public class SlackTextBlock: SlackBlock
    {
        private readonly string _text;

        public SlackTextBlock(string text)
        {
            _text = text;
        }

        public override JObject ToJson()
        {
            var ret = new JObject();
            ret.Add("type", "mrkdwn");
            ret.Add("text", _text);
            return ret;
        }
    }
}