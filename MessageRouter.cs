using Akka.Actor;

namespace gamemaster
{
    public class MessageRouter
    {
        private IActorRef _slackGateway;

        public void ToSlackGateway<T>(T message)
        {
            _slackGateway.Tell(message, Nobody.Instance);
        }
        
        public void RegisterSlackGateway(IActorRef self)
        {
            _slackGateway = self;
        }
    }
}