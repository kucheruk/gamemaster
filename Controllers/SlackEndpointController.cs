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

        public SlackEndpointController(ILogger<SlackEndpointController> logger, IOptions<SlackConfig> cfg)
        {
            _logger = logger;
            _cfg = cfg;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}