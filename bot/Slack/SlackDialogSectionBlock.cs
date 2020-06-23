using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackDialogSectionBlock : SlackDialogBlock
    {
        public SlackDialogSectionBlock(string text, bool mrkdwn = false) : base("section")
        {
            Text = new PlainTextSlackElement(text, mrkdwn ? "mrkdwn" : "plain_text");
        }
        
        [JsonProperty("text")]
        public PlainTextSlackElement Text { get; set; }
    }
}