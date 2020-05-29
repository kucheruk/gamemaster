using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gamemaster.Controllers
{
    [ApiController]
    [Route("api/slack")]
    public class SlackEndpointController : ControllerBase
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly ILogger<SlackEndpointController> _logger;
        private readonly MessageRouter _router;

        public SlackEndpointController(ILogger<SlackEndpointController> logger, IOptions<SlackConfig> cfg,
            MessageRouter router)
        {
            _logger = logger;
            _cfg = cfg;
            _router = router;
        }


        [HttpPost]
        public IActionResult Post(dynamic msg)
        {
            if (msg.type == "url_verification")
            {
                if (msg.token == _cfg.Value.VerificationToken)
                {
                    return Ok(new {Challenge = msg.challenge});
                }
            }

            if (msg.type == "event_callback")
            {
                HandleEvent(msg);
            }
            else
            {
                
            }

            return Ok();
        }

        private void HandleEvent(dynamic msg)
        {
            if (msg.@event.type == "message")
            {
                string botId = msg.@event.bot_id;
                if (!String.IsNullOrEmpty(botId))
                {
                    return; // quick and dirty: ignore self (message loop)
                }
                string txt = msg.@event.text;
                string author = msg.@event.user;
                _router.ToSlackGateway(new MessageToChannel(author, $"echo {txt}"));
            }
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}