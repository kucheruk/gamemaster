using System.Collections.Generic;
using Newtonsoft.Json;

namespace gamemaster.Slack
{
    public class SlackInteractionPayload
    {

        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("team")]
        public SlackInteractionTeam Team { get; set; }
        
        [JsonProperty("user")]
        public SlackInteractionUser User { get; set; }

        [JsonProperty("api_app_id")]
        public string ApiAppId { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("container")]
        public SlackInteractionContainer Container { get; set; }

        [JsonProperty("trigger_id")]
        public string TriggerId { get; set; }

        
        [JsonProperty("channel")]
        public SlackInteractionChannel Channel { get; set; }

        [JsonProperty("response_url")]
        public string ResponseUrl { get; set; }

        public SlackDialogPayloadView View {
            get;
            set;
        }
        
        [JsonProperty("actions")]
        public IList<SlackInteractionAction> Actions { get; set; }
    }

    public class SlackDialogPayloadView
    {
        [JsonProperty("callback_id")]
        public string CallbackId { get; set; }
        
        [JsonProperty("state")]
        public StateDictionaryWrapper State { get; set; }
    }

    public class StateDictionaryWrapper
    {
        [JsonProperty("values")]
        public Dictionary<string, Dictionary<string, SlackInputValue>> Values { get; set; }
    }

    public class SlackInputValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("selected_option")]
        public RadioButtonSelectedOptionValue SelectedOption { get; set; }
        
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class RadioButtonSelectedOptionValue
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}