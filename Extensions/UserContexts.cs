using Akka.Actor;
using Akka.DI.Core;
using gamemaster.Messages;

namespace gamemaster.Extensions
{
    public class UserContextsActor : ReceiveActor
    {
        private readonly MessageRouter _router;
        
        public UserContextsActor(MessageRouter router)
        {
            _router = router;
            Receive<PlaceBetMessage>(PlaceBetAmount);
            Receive<PlaceBetStartMessage>(StartBetDialog);
            Receive<PlaceBetSelectOptionMessage>(SelectNumber);
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
        
        private bool SelectNumber(PlaceBetSelectOptionMessage msg)
        {
            var child = Context.Child($"bet_{msg.UserId}");
            if (child.IsNobody())
            {
                _router.ToSlackGateway(new MessageToChannel(msg.UserId, "Время выбора деталей ставки истекло. Нажми-ка кнопку для участия в тотализаторе ещё раз."));
            }
            else
            {
                child.Forward(msg);
            }

            return true;
        }
        private bool PlaceBetAmount(PlaceBetMessage msg)
        {
            var child = Context.Child($"bet_{msg.UserId}");
            if (child.IsNobody())
            {
                _router.ToSlackGateway(new MessageToChannel(msg.UserId, "Время выбора деталей ставки истекло. Нажми-ка кнопку для участия в тотализаторе ещё раз."));
            }
            else
            {
                child.Forward(msg);
            }

            return true;
        }
    }
}