using Newtonsoft.Json;

namespace gamemaster.Slack
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