using Newtonsoft.Json.Linq;

namespace gamemaster.CommandHandlers
{
    public sealed class SectionTextSlackBlock : SlackBlock
    
    {
        private SlackTextBlock _text;

        public SectionTextSlackBlock(SlackTextBlock text)
        {
            _text = text;
        }

        public override JObject ToJson()
        {
            var ret = new JObject {{"type", "section"}, {"text", _text.ToJson()}};
            return ret;
        }
    }
}