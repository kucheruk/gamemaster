using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public abstract class SlackDialogBlock
    {
        protected SlackDialogBlock(string type)
        {
            Type = type;
        }

        [JsonProperty("type")] public string Type { get; set; }
    }
}