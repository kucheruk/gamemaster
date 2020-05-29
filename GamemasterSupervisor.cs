using Akka.Actor;

namespace gamemaster
{
    public class GamemasterSupervisor: ReceiveActor
    {
        public GamemasterSupervisor()
        {
            
        }

        protected override void PreStart()
        {
            base.PreStart();
            Context.ChildWithBackoffSupervision<SlackApiConnectionActor>();
        }
    }
}