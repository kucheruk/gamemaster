using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackDialogModel
    {
        public SlackDialogModel(string title, string submit, string callbackId)
        {
            CallbackId = callbackId;
            Title = new PlainTextSlackElement(title);
            Submit = new PlainTextSlackElement(submit);
        }
        
        [JsonProperty("title")]
        public PlainTextSlackElement Title { get; set; }
        
        [JsonProperty("callback_id")]
        public string CallbackId { get; set; }
        
        [JsonProperty("submit")]
        public PlainTextSlackElement Submit { get; set; }
        
        [JsonProperty("blocks")]
        public SlackDialogBlock[] Blocks { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "modal";
    }
}