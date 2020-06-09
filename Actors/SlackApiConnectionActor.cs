using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Messages;

namespace gamemaster.Actors
{
    public class SlackApiConnectionActor : ReceiveActor
    {
        private readonly MessageRouter _router;
        private readonly SlackApiWrapper _slack;

        public SlackApiConnectionActor(MessageRouter router, SlackApiWrapper slack)
        {
            _router = router;
            _slack = slack;
            ReceiveAsync<MessageToChannel>(SendMessage);
            ReceiveAsync<BlocksMessage>(SendBlocksMessage);
            ReceiveAsync<GetChannelUsersRequestMessage>(GetUsers);
        }

        private async Task SendBlocksMessage(BlocksMessage arg)
        {
            await _slack.PostAsync(arg.ChannelId, arg.Blocks);
        }


        private async Task SendMessage(MessageToChannel obj)
        {
            await _slack.PostAsync(obj);
        }
      
        private async Task GetUsers(GetChannelUsersRequestMessage msg)
        {
            var reply = await _slack.GetChannelMembers(msg.Context);
            Sender.Tell(reply);
        }


        protected override void PreStart()
        {
            base.PreStart();
            _router.RegisterSlackGateway(Self);
            _slack.IAmOnline();
        }
    }
}