using Akka.Actor;
using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackDialogInputBlock : SlackDialogBlock
    {
        public SlackDialogInputBlock(SlackDialogElement element, string label) : base("input")
        {
            Element = element;
            Label = new PlainTextSlackElement(label);
        }

        [JsonProperty("label")] public PlainTextSlackElement Label { get; set; }

        [JsonProperty("element")] public SlackDialogElement Element { get; set; }
    }
}