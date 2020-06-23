using System.Linq;

namespace gamemaster.CommandHandlers
{
    public class TossRequestParams
    {
        public string UserId { get; }
        public string Currency { get; }
        public decimal Amount { get; }
        public string Comment { get; }

        private TossRequestParams(string userId, string currency,
            in decimal amount, string comment)
        {
            UserId = userId;
            Currency = currency;
            Amount = amount;
            Comment = comment;
        }

        public static TossRequestParams FromText(string text)
        {
            var parts = text.Trim().Split(' ').ToHashSet();
            if (parts.Count <= 1)
            {
                return null;
            }

            var currency = CommandsPartsParse.FindCurrency(parts, Constants.DefaultCurrency);

            parts.Remove(currency);

            var userId = CommandsPartsParse.FindUserId(parts);
            if (userId.HasValue)
            {
                parts.Remove(userId.Value.part);
            }

            var (amountstr, amount) = CommandsPartsParse.FindDecimal(parts, 0);

            parts.Remove(amountstr);

            var comment = string.Join(' ', parts).Trim();

            return new TossRequestParams(userId?.id, currency, amount, comment);

        }

    }
}