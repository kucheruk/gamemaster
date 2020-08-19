using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace gamemaster
{
    public abstract class SlackJsonHandler
    {
        public abstract Task<bool> Handle(JObject rq, HttpResponse response);
    }
}