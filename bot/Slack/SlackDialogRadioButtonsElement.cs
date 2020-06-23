using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackDialogRadioButtonsElement : SlackDialogElement
    {
        [JsonProperty("action_id")]
        public string ActionId { get; }

        public SlackDialogRadioButtonsElement(string actionId, Dictionary<string, string> values) : base("radio_buttons")
        {
            ActionId = actionId;
            Options = values.Select(a => new SlackOptionElement(a.Value, a.Key)).ToArray();
        }

        [JsonProperty("options")] public SlackOptionElement[] Options { get; set; }
    }
}