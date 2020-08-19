using gamemaster.Slack.Models;
using Newtonsoft.Json;

namespace gamemaster.Slack.Dialogs
{
    public class SlackDialogPlainTextInputElement : SlackDialogElement
    {
        public SlackDialogPlainTextInputElement(string actionId, string placeholder) : base("plain_text_input")
        {
            ActionId = actionId;
            Placeholder = new PlainTextSlackElement(placeholder);
        }
        
        [JsonProperty("action_id")]
        public string ActionId { get; set; }
        
        [JsonProperty("placeholder")]
        public PlainTextSlackElement Placeholder { get; set; }
    }
}