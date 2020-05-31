using System.Linq;
using System.Text.RegularExpressions;

namespace gamemaster.CommandHandlers
{
    public static class CommandsPartsParse
    {
        public static readonly Regex IntRx = new Regex("^\\d+$", RegexOptions.Compiled);
        private static readonly Regex UserRx = new Regex("^<@([^|]+)\\|(.*?)>$");

        public static int FindInteger(string[] parts, int def)
        {
            var p = parts.FirstOrDefault(IntRx.IsMatch);
            if (p != null)
            {
                return int.Parse(p);
            }

            return def;
        }

        public static string FindCurrency(string[] parts, string defaultCurrency)
        {
            return parts.FirstOrDefault(a => a.StartsWith(':') && a.EndsWith(':')) ?? defaultCurrency;
        }

        //<@U033GDN1S|kucheruk>
        public static (string id, string name)? FindUserId(string[] parts)
        {
            var uid = parts.FirstOrDefault(UserRx.IsMatch);
            if (uid == null)
            {
                return null;
            }

            var p = UserRx.Match(uid);
            return (p.Groups[1].ToString(), p.Groups[2].ToString());
        }

    }
}