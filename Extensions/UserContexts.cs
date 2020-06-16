using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using Akka.Event;
using gamemaster.Actors;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;

namespace gamemaster.Extensions
{
    public class UserContextsActor : ReceiveActor
    {
        private readonly MessageRouter _router;
        private readonly SlackApiWrapper _slack;
        private ILogger<UserContextsActor> _logger;

        public UserContextsActor(MessageRouter router,
            SlackApiWrapper slack,
            ILogger<UserContextsActor> logger)
        {
            _router = router;
            _slack = slack;
            _logger = logger;
            ReceiveAsync<PlaceBetMessage>(PlaceBetAmount);
            Receive<PlaceBetStartMessage>(StartBetDialog);
            ReceiveAsync<PlaceBetSelectOptionMessage>(SelectNumber);
        }

        protected override void PreStart()
        {
            _router.RegisterUserContextsActor(Self);
            base.PreStart();
        }

        private void StartBetDialog(PlaceBetStartMessage msg)
        {
            var name = $"bet_{msg.UserId}";
            var child = Context.Child(name);
            if (child.IsNobody())
            {
                child = Context.ActorOf(Context.DI().Props<UserToteContextActor>(), name);
            }
            child.Forward(msg);
        }
        
        private async Task<bool> SelectNumber(PlaceBetSelectOptionMessage msg)
        {
            var child = Context.Child($"bet_{msg.UserId}");
            if (child.IsNobody())
            {
                await _slack.PostAsync(new MessageToChannel(msg.UserId, "Время выбора деталей ставки истекло. Нажми-ка кнопку для участия в тотализаторе ещё раз."));
            }
            else
            {
                child.Forward(msg);
            }

            return true;
        }
        private async Task<bool> PlaceBetAmount(PlaceBetMessage msg)
        {
            _logger.LogInformation("Place bet {user} {text}", msg.UserId, msg.Text);
            var child = Context.Child($"bet_{msg.UserId}");
            if (child.IsNobody())
            {
                await _slack.PostAsync(new MessageToChannel(msg.UserId, "Время выбора деталей ставки истекло. Нажми-ка кнопку для участия в тотализаторе ещё раз."));
            }
            else
            {
                child.Forward(msg);
            }

            return true;
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, "Error");
            base.PreRestart(reason, message);
        }
    }
}