using System.Threading.Tasks;
using gamemaster.Config;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace gamemaster.Slack.Handlers
{
    public class SlackUrlVerificationHandler : SlackJsonHandler
    {
        private readonly IOptions<SlackConfig> _cfg;

        public SlackUrlVerificationHandler(IOptions<SlackConfig> cfg)
        {
            _cfg = cfg;
        }

        public override async Task<bool> Handle(SlackRequestContainer req)
        {
            if (req.Json["type"]?.ToString() != "url_verification")
            {
                return false;
            }

            await ResponseChallenge(req.Response, req.Json["challenge"]?.ToString(),
                req.Json["token"]?.ToString());
            return true;
        }
        
        private async Task ResponseChallenge(HttpResponse resp, string challenge,
            string token)
        {
            if (_cfg.Value.VerificationToken == token)
            {
                resp.StatusCode = 200;
                await resp.StartAsync();
                await resp.WriteAsync(challenge);
            }
        }

    }
}