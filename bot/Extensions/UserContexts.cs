using System;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.DI.Core;
using gamemaster.Actors;
using gamemaster.Messages;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;

namespace gamemaster.Extensions
{
    public class UserContextsActor : ReceiveActor
    {
        private readonly ILogger<UserContextsActor> _logger;
        private readonly SlackApiWrapper _slack;

        public UserContextsActor(
            SlackApiWrapper slack,
            ILogger<UserContextsActor> logger)
        {
            _slack = slack;
            _logger = logger;
            Receive<PlaceBetStartMessage>(StartBetDialog);
            Receive<PlaceBetSelectOptionMessage>(SelectNumber);
        }

        public static IActorRef Address { get; private set; }

        protected override void PreStart()
        {
            Address = Self;
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

        private bool SelectNumber(PlaceBetSelectOptionMessage msg)
        {
            var child = Context.Child($"bet_{msg.UserId}");
            if (child.IsNobody())
            {
                MessengerActor.Send(new MessageToChannel(msg.UserId, "Время выбора деталей ставки истекло. Нажми-ка кнопку для участия в тотализаторе ещё раз."));
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