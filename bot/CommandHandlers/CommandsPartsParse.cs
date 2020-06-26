using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using gamemaster.Extensions;

namespace gamemaster.CommandHandlers
{
    public static class CommandsPartsParse
    {
        private static readonly Regex UserRx = new Regex("^<@([^|]+)\\|(.*?)>$");

        public static (string p, decimal) FindDecimal(IEnumerable<string> parts, int def)
        {
            var p = parts.FirstOrDefault(a => decimal.TryParse(a, out var v) && v > 0);
            if (p != null)
            {
                return (p, decimal.Parse(p).Trim());
            }

            return (string.Empty, def);
        }

        public static string FindCurrency(IEnumerable<string> parts, string defaultCurrency)
        {
            return parts.FirstOrDefault(a => a.StartsWith(':') && a.EndsWith(':')) ?? defaultCurrency;
        }

        //<@U033GDN1S|kucheruk>
        public static (string id, string part)? FindUserId(IEnumerable<string> parts)
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