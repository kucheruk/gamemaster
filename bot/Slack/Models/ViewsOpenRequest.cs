using gamemaster.Slack.Dialogs;
using Newtonsoft.Json;

namespace gamemaster.Slack.Models
{
    public class ViewsOpenRequest
    {
        public ViewsOpenRequest(string triggerId, SlackDialogModel view)
        {
            TriggerId = triggerId;
            View = view;
        }

        [JsonProperty("trigger_id")]
        public string TriggerId { get; set; }
        
        [JsonProperty("view")]
        public SlackDialogModel View { get; set; }
    }
}