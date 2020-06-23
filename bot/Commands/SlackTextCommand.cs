namespace gamemaster.CommandHandlers
{
    public class SlackTextCommand
    {
        public SlackTextCommand(string userId, MessageContext context,
            string responseUrl, string text)
        {
            UserId = userId;
            Context = context;
            ResponseUrl = responseUrl;
            Text = text;
        }

        public string UserId { get; }
        public MessageContext Context { get; }
        public string ResponseUrl { get; }
        public string Text { get; }
    }
}