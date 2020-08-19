using Newtonsoft.Json;

namespace gamemaster.Slack.Dialogs
{
    public abstract class SlackDialogElement
    {
        protected SlackDialogElement(string type)
        {
            Type = type;
        }

        [JsonProperty("type")] public string Type { get; set; }
    }
}