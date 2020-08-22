using System.Net;
using System.Threading.Tasks;
using gamemaster.Slack;
using Microsoft.AspNetCore.Http;

namespace gamemaster
{
    public class CheckSlackSignatureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SlackRequestSignature _slackSignature;
        private readonly SlackRequestContainer _req;

        public CheckSlackSignatureMiddleware(RequestDelegate next, SlackRequestSignature slackSignature, SlackRequestContainer req)
        {
            _next = next;
            _slackSignature = slackSignature;
            _req = req;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var bodyAsText = await context.ReadRequestBodyAstString();
            _req.Raw = bodyAsText;
            if (SignatureKeyIsValid(bodyAsText, request))
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsync("{\"error\":\"signature_invalid\"}");
            }
        }

        private bool SignatureKeyIsValid(string bodyAsText, HttpRequest request)
        {
            if (request.Headers.TryGetValue("X-Slack-Request-Timestamp", out var timestamp))
            {
                if (request.Headers.TryGetValue("X-Slack-Signature", out var signature))
                {
                    return _slackSignature.Validate(bodyAsText, timestamp, signature);
                }
            }
            return false;
        }
    }
}