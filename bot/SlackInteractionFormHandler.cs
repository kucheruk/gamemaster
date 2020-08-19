using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Extensions;
using gamemaster.Messages;
using gamemaster.Slack;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace gamemaster
{
    public class SlackInteractionFormHandler : SlackFormHandler
    {
        private readonly IEnumerable<SlackInteractionActionHandler> _handlers;

        public SlackInteractionFormHandler(IEnumerable<SlackInteractionActionHandler> handlers)
        {
            _handlers = handlers;
        }

        public override async Task<bool> Handle(Dictionary<string, string> form, HttpResponse resp)
        {
            if (form.TryGetValue("payload", out var payload))
            {
                var pl = DeserializePayload(payload);
                HandleInteraction(pl);
                resp.StatusCode = 200;
                await resp.CompleteAsync();
                return true;
            }

            return false;
        }

        private void HandleInteraction(SlackInteractionPayload pl)
        {
            if (pl?.Actions?.Count > 0)
            {
                foreach (var action in pl.Actions)
                {
                    foreach (var handler in _handlers)
                    {
                        handler.Handle(action.ActionId, pl.User.Id, pl.ResponseUrl, pl.TriggerId);
                    }
                }
            }
            else
            {
                var values = pl?.View?.State?.Values;
                if (values == null)
                {
                    return;
                }

                var v = values;
                var vals = v.SelectMany(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
                if (!vals.TryGetValue("bet_option", out var option) || !vals.TryGetValue("bet_amount", out var amount))
                {
                    return;
                }

                var cb = pl.View.CallbackId;
                if (string.IsNullOrEmpty(cb))
                {
                    return;
                }

                var parts = cb.Split(':');
                if (parts.Length <= 1)
                {
                    return;
                }

                var toteId = parts[1];
                var userId = pl.User.Id;
                var optId = option.SelectedOption.Value;
                var am = decimal.Parse(amount.Value).Trim();
                TotesActor.Address.Tell(new TotePlaceBetMessage(userId, toteId, optId, am, pl?.Channel?.Id));
            }
        }

        private SlackInteractionPayload DeserializePayload(string payload)
        {
            return JsonConvert.DeserializeObject<SlackInteractionPayload>(payload);
        }
    }
}