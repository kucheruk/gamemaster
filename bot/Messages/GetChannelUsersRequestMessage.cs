namespace gamemaster.Messages
{
    public class GetChannelUsersRequestMessage
    {
        public MessageContext Context { get; }

        public GetChannelUsersRequestMessage(MessageContext context)
        {
            Context = context;
        }
    }
}