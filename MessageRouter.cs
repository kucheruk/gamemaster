using System;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;

namespace gamemaster
{
    public class MessageRouter
    {
        private readonly ILogger<MessageRouter> _logger;
        private ActorSystem _as;
        private IActorRef _ledger;
        private IActorRef _slackGateway;

        public MessageRouter(ILogger<MessageRouter> logger)
        {
            _logger = logger;
        }

        public void ToSlackGateway<T>(T message)
        {
            _slackGateway.Tell(message, Nobody.Instance);
        }

        public void RegisterSystem(ActorSystem sys)
        {
            _as = sys;
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
            in decimal amount, string adminId,
            string responseUrl)
        {
            _ledger.Tell(new EmitMessage(toUser, currency, amount, adminId, responseUrl));
        }


        public void LedgerBalance(GetBalanceMessage msg)
        {
            _ledger.Tell(msg);
        }

        public void LedgerToss(TossACoinMessage msg)
        {
            _ledger.Tell(msg);
        }

        public async Task<TResp> RpcToSlack<TQuery, TResp>(TQuery msg) where TResp : class
        {
            var inbox = Inbox.Create(_as);
            inbox.Send(_slackGateway, msg);

            try
            {
                var ret = await inbox.ReceiveAsync(TimeSpan.FromSeconds(2));
                return ret as TResp;
            }
            catch (TimeoutException)
            {
                _logger.LogError("Slack RPC timeout {Message}", msg);
            }

            return null;
        }

        public void LedgerGiveAway(GiveAwayMessage msg)
        {
            _ledger.Tell(msg);
        }

        public void LedgerCancelTote(ToteCancelledMessage msg)
        {
            _ledger.Tell(msg);
        }

        public void LedgerFinishTote(ToteFinishedMessage msg)
        {
            _ledger.Tell(msg);
        }

        public void LedgerPlaceBet(TotePlaceBetMessage msg)
        {
            _ledger.Tell(msg);
        }
    }
}