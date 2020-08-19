using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace gamemaster.Slack.Handlers
{
    public abstract class SlackFormHandler
    {
        public abstract Task<bool> Handle(Dictionary<string, string> form, HttpResponse resp);
    }
}