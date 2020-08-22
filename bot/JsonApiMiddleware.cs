using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using gamemaster.Slack;
using gamemaster.Slack.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class JsonApiMiddleware
    {
        private readonly SlackRequestContainer _req;
        private readonly IEnumerable<SlackJsonHandler> _jsonHandlers;
        private readonly IEnumerable<SlackFormHandler> _formHandlers;
        private readonly ILogger<JsonApiMiddleware> _logger;

        public JsonApiMiddleware(RequestDelegate _,
            SlackRequestContainer req,
            IEnumerable<SlackJsonHandler> jsonHandlers,
            ILogger<JsonApiMiddleware> logger,
            IEnumerable<SlackFormHandler> formHandlers)
        {
            _req = req;
            _jsonHandlers = jsonHandlers;
            _logger = logger;
            _formHandlers = formHandlers;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var mediaType = GetMediaType(request);
                if (mediaType == "application/json")
                {
                    var json = JObject.Parse(_req.Raw);
                    _req.Json = json;
                    foreach (var jsonHandler in _jsonHandlers)
                    {
                        if (await jsonHandler.Handle(json, context.Response))
                        {
                            return;
                        }
                    }
                }
                else if (mediaType == "application/x-www-form-urlencoded")
                {
                    var form = _req.Raw.Split("&").Select(a => a.Split("="))
                        .ToDictionary(a => a[0], a => HttpUtility.UrlDecode(a[1]));
                    _req.Form = form;
                    foreach (var formHandler in _formHandlers)
                    {
                        if (await formHandler.Handle(form, context.Response))
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling request");
            }
        }

        private static StringSegment GetMediaType(HttpRequest request)
        {
            return request.GetTypedHeaders().ContentType.MediaType;
        }

    }
}