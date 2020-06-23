using gamemaster.Models;

namespace gamemaster.CommandHandlers
{
    public class ToteStatusMessage
    {
        public MessageContext Context { get; }
        public Tote Tote { get; }

        public ToteStatusMessage(MessageContext context, Tote tote)
        {
            Context = context;
            Tote = tote;
        }
    }
}