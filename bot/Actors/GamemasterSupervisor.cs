using Akka.Actor;
using gamemaster.Extensions;
using gamemaster.Messages;
using gamemaster.Services;
using Microsoft.Extensions.Logging;

namespace gamemaster.Actors
{
    public class GamemasterSupervisor : ReceiveActor
    {
        private readonly ILogger<GamemasterSupervisor> _logger;

        public GamemasterSupervisor(ILogger<GamemasterSupervisor> logger)
        {
            _logger = logger;
            Receive<DbMainetanceDoneMessage>(StartupSystem);
        }

        private void StartupSystem(DbMainetanceDoneMessage arg)
        {
            _logger.LogInformation("Starting Actors");
            Context.ChildWithBackoffSupervision<LedgerActor>();
            Context.ChildWithBackoffSupervision<UserContextsActor>();
            Context.ChildWithBackoffSupervision<TotesActor>();
            Context.ChildWithBackoffSupervision<MessengerActor>();
        }

        protected override void PreStart()
        {
            _logger.LogInformation("Starting Supervisort");
            Context.ChildWithBackoffSupervision<DbMaintenanceService>();
            base.PreStart();
        }
    }
}