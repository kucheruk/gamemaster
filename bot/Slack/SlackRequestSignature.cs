using System;
using System.Security.Cryptography;
using System.Text;
using gamemaster.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace gamemaster.Slack
{
    public class SlackRequestSignature
    {
        private const string ApiVer = "v0";
        private readonly IOptions<SlackConfig> _cfg;

        public SlackRequestSignature(IOptions<SlackConfig> cfg)
        {
            _cfg = cfg;
        }

        public bool Validate(string requestBody, StringValues timestamp,
            StringValues signature)
        {
            var keyContent = $"{ApiVer}:{timestamp}:{requestBody}";
            var key = $"{ApiVer}={GetHmacSha256Hash(keyContent, _cfg.Value.SigningSecret)}";
            return signature == key;
        }

        private static string GetHmacSha256Hash(string text, string key)
        {
            var encoding = new UTF8Encoding();
            var textBytes = encoding.GetBytes(text);
            var keyBytes = encoding.GetBytes(key);
            byte[] hashBytes;

            using (var hash = new HMACSHA256(keyBytes))
            {
                hashBytes = hash.ComputeHash(textBytes);
            }

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}