using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public class JsonApiMiddleware
    {
        private readonly IEnumerable<SlackJsonHandler> _jsonHandlers;
        private readonly IEnumerable<SlackFormHandler> _formHandlers;
        private readonly ILogger<JsonApiMiddleware> _logger;

        public JsonApiMiddleware(RequestDelegate _,
            IEnumerable<SlackJsonHandler> jsonHandlers,
            ILogger<JsonApiMiddleware> logger,
            IEnumerable<SlackFormHandler> formHandlers)
        {
            _jsonHandlers = jsonHandlers;
            _logger = logger;
            _formHandlers = formHandlers;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var bodyAsText = await context.ReadRequestBodyAstString();
                var mediaType = GetMediaType(request);
                if (mediaType == "application/json")
                {
                    var rq = JObject.Parse(bodyAsText);
                    foreach (var jsonHandler in _jsonHandlers)
                    {
                        if (await jsonHandler.Handle(rq, context.Response))
                        {
                            return;
                        }
                    }
                }
                else if (mediaType == "application/x-www-form-urlencoded")
                {
                    var form = bodyAsText.Split("&").Select(a => a.Split("="))
                        .ToDictionary(a => a[0], a => HttpUtility.UrlDecode(a[1]));
                  
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