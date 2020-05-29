using Akka.Actor;

namespace gamemaster
{
    public class GamemasterSupervisor : ReceiveActor
    {
        protected override void PreStart()
        {
            Context.ChildWithBackoffSupervision<SlackApiConnectionActor>();
            base.PreStart();
        }
    }
}