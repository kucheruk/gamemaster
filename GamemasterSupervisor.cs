using Akka.Actor;

namespace gamemaster
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