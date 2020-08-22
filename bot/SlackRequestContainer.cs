using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class SlackRequestContainer
    {
        public string Raw { get; set; }
        public JObject Json { get; set; }
        public Dictionary<string, string> Form { get; set; }
    }
}