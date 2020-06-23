using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace gamemaster
{
    public class DebugOnlyReadRequestBodyIntoItemsAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        public const string RequestBodyKey = "request_body";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var syncIoFeature = context?.HttpContext.Features.Get<IHttpBodyControlFeature>();
            if (syncIoFeature != null)
            {
                syncIoFeature.AllowSynchronousIO = true;
                var req = context.HttpContext.Request;
                req.EnableBuffering();
                if (req.Body.CanSeek)
                {
                    req.Body.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(
                        req.Body,
                        Encoding.UTF8,
                        false,
                        8192,
                        true))
                    {
                        var jsonString = reader.ReadToEnd();
                        context.HttpContext.Items.Add(RequestBodyKey, jsonString);
                    }

                    req.Body.Seek(0, SeekOrigin.Begin);
                }
            }
        }
    }
}