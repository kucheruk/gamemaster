using System.Linq;
using System.Text.RegularExpressions;

namespace gamemaster.CommandHandlers
{
    public static class CommandsPartsParse
    {
        private static readonly Regex UserRx = new Regex("^<@([^|]+)\\|(.*?)>$");

        public static (string p, decimal) FindDecimal(string[] parts, int def)
        {
            var p = parts.FirstOrDefault(a => decimal.TryParse(a, out _));
            if (p != null)
            {
                return (p, decimal.Round(decimal.Parse(p), 2));
            }

            return (string.Empty, def);
        }

        public static string FindCurrency(string[] parts, string defaultCurrency)
        {
            return parts.FirstOrDefault(a => a.StartsWith(':') && a.EndsWith(':')) ?? defaultCurrency;
        }

        //<@U033GDN1S|kucheruk>
        public static (string id, string part)? FindUserId(string[] parts)
        {
            var uid = parts.FirstOrDefault(UserRx.IsMatch);
            if (uid == null)
            {
                return null;
            }

            var p = UserRx.Match(uid);
            return (p.Groups[1].ToString(), uid);
        }
    }
}