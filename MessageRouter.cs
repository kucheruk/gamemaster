using Akka.Actor;
using gamemaster.Messages;

namespace gamemaster
{
    public class MessageRouter
    {
        private IActorRef _ledger;
        private IActorRef _slackGateway;

        public void ToSlackGateway<T>(T message)
        {
            _slackGateway.Tell(message, Nobody.Instance);
        }

        public void RegisterSlackGateway(IActorRef self)
        {
            _slackGateway = self;
        }


        public void RegisterLedger(IActorRef aref)
        {
            _ledger = aref;
        }

        public void LedgerEmit(string toUser, string currency,
            in int amount, string adminId,
            string responseUrl)
        {
            _ledger.Tell(new EmitMessage(toUser, currency, amount, adminId, responseUrl));
        }

        
        public void LedgerBalance(GetBalanceMessage msg)
        {
            _ledger.Tell(msg);
        }
    }
}