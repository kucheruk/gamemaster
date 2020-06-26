using System.Linq;
using gamemaster.CommandHandlers;

namespace gamemaster.Models
{
    public class TossRequestParams
    {
        public string UserId { get; }
        public string Currency { get; }
        public bool TossAll { get; }
        public decimal Amount { get; }
        public string Comment { get; }

        private TossRequestParams(string userId, string currency,
            in decimal amount, string comment,
            bool all)
        {
            UserId = userId;
            Currency = currency;
            Amount = amount;
            Comment = comment;
            TossAll = all;
        }

        public static TossRequestParams FromText(string text)
        {
            var all = false;
            var parts = text.Trim().Split(' ').ToArray();
            if (parts.Length <= 1)
            {
                return null;
            }
            var currency = CommandsPartsParse.FindCurrency(parts, Constants.DefaultCurrency);
            var rest = text.Replace(currency, string.Empty);
            var userId = CommandsPartsParse.FindUserId(parts);
            if (userId.HasValue)
            {
                rest = rest.Replace(userId.Value.part, string.Empty);
            }
            var (amountstr, amount) = CommandsPartsParse.FindDecimal(parts, 0);
            
            if (amount > 0)
            {
                rest = rest.Replace(amountstr, string.Empty);
            }
            else
            {
                if (parts.Any(a => a == "*"))
                {
                    all = true;
                }
            }

            var comment = rest.Trim();
            return new TossRequestParams(userId?.id, currency, amount, comment, all);
        }

    }
}