using System;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Extensions;
using gamemaster.Messages;
using Microsoft.Extensions.Logging;

namespace gamemaster
{
    public class MessageRouter
    {
        private readonly ILogger<MessageRouter> _logger;
        private ActorSystem _as;
        private IActorRef _ledger;
        private IActorRef _userContexts;
        public IActorRef Messenger { get; private set; }

        public MessageRouter(ILogger<MessageRouter> logger)
        {
            _logger = logger;
        }

    
        public void RegisterSystem(ActorSystem sys)
        {
            _as = sys;
        }


        public void RegisterMessenger(IActorRef aref)
        {
            Messenger = aref;
        }
        
        public void RegisterLedger(IActorRef aref)
        {
            _ledger = aref;
        }

        public void RegisterTotes(IActorRef aref)
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
            if (_ledger.IsNobody())
            {
                _logger.LogError("Ledger ref missing");
            }
            _ledger.Tell(msg);
        }

        public void RegisterUserContextsActor(IActorRef @ref)
        {
            _userContexts = @ref;
        }

        public void StartBetProcess(PlaceBetStartMessage msg)
        {
            _userContexts.Tell(msg);
        }

        public void SelectBetOption(PlaceBetSelectOptionMessage msg)
        {
            _userContexts.Tell(msg);
        }

        public void BetInfo(PlaceBetMessage msg)
        {
            _userContexts.Tell(msg);
        }

        public void ToteStatus(ToteStatusMessage msg)
        {
            _ledger.Tell(msg);
        }
    }
}