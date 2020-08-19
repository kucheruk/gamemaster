using System.Collections.Generic;
using System.Diagnostics;
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
        public override async Task<bool> Handle(Dictionary<string, string> form, HttpResponse resp)
        {
            if (form.TryGetValue("payload", out var payload))
            {
                var sw = new Stopwatch();
                sw.Start();
                var pl = DeserializePayload(payload);
                HandleInteraction(pl);
                resp.StatusCode = 200;
                await resp.CompleteAsync();
                sw.Stop();
            }
        }

        private void HandleInteraction(SlackInteractionPayload pl)
        {
            if (pl?.Actions?.Count > 0)
            {
                foreach (var action in pl.Actions)
                {
                    HandleInteractionAction(action, pl.User.Id, pl.ResponseUrl, pl.TriggerId);
                }
            }
            else
            {
                var values = pl?.View?.State?.Values;
                if (values != null)
                {
                    var v = values;
                    var vals = v.SelectMany(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
                    if (vals.TryGetValue("bet_option", out var option) && vals.TryGetValue("bet_amount", out var amount))
                    {
                        var cb = pl.View.CallbackId;
                        if (!string.IsNullOrEmpty(cb))
                        {
                            var parts = cb.Split(':');
                            if (parts.Length > 1)
                            {
                                var toteId = parts[1];
                                var userId = pl.User.Id;
                                var optId = option.SelectedOption.Value;
                                var am = decimal.Parse(amount.Value).Trim();
                                TotesActor.Address.Tell(new TotePlaceBetMessage(userId, toteId, optId, am, pl?.Channel?.Id));
                            }
                        }
                    }
                }
            }
        }

        private void HandleInteractionAction(SlackInteractionAction action, string userId,
            string plResponseUrl, string triggerId)
        {
            if (action.ActionId.StartsWith("finish_tote"))
            {
                var parts = action.ActionId.Split(':');
                HandleFinishTote(parts[1], parts[2], userId);
            }

            if (action.ActionId.StartsWith("start_bet"))
            {
                var parts = action.ActionId.Split(':');
                HandleStartBet(parts[1], userId, plResponseUrl, triggerId);
            }

            if (action.ActionId.StartsWith("option_select"))
            {
                var parts = action.ActionId.Split(':');
                HandleSelectNumber(parts[1], parts[2], userId);
            }
        }

        private void HandleStartBet(string toteId, string userId,
            string plResponseUrl, string triggerId)
        {
            UserContextsActor.Address.Tell(new PlaceBetStartMessage(userId, toteId, plResponseUrl, triggerId));
        }

        private void HandleSelectNumber(string toteId, string optionId,
            string userId)
        {
            UserContextsActor.Address.Tell(new PlaceBetSelectOptionMessage(userId, toteId, optionId));
        }

        private void HandleFinishTote(string toteId, string optionId,
            string userId)
        {
            TotesActor.Address.Tell(new ToteFinishedMessage(toteId, optionId, userId));
        }


        private SlackInteractionPayload DeserializePayload(string payload)
        {
            return JsonConvert.DeserializeObject<SlackInteractionPayload>(payload);
        }
    }
}