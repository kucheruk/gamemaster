using Akka.Actor;
using gamemaster.Extensions;
using gamemaster.Messages;
using gamemaster.Services;

namespace gamemaster.Actors
{
    public class GamemasterSupervisor : ReceiveActor
    {
        public GamemasterSupervisor()
        {
            Receive<DbMainetanceDoneMessage>(StartupSystem);
        }

        private void StartupSystem(DbMainetanceDoneMessage arg)
        {
            Context.ChildWithBackoffSupervision<SlackApiConnectionActor>();
            Context.ChildWithBackoffSupervision<LedgerActor>();
        }

        protected override void PreStart()
        {
            Context.ChildWithBackoffSupervision<DbMaintenanceService>();
            base.PreStart();
        }
    }
}