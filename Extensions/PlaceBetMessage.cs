namespace gamemaster.Extensions
{
    public class PlaceBetMessage
    {
        public PlaceBetMessage(string userId, string text)
        {
            Text = text;
            UserId = userId;
        }

        public string Text { get;  }
        public string UserId { get; }
    }
}