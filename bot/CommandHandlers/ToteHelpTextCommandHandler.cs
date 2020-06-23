using System.Threading.Tasks;

namespace gamemaster.CommandHandlers
{
    public class ToteHelpTextCommandHandler : ITextCommandHandler
    {
        public bool Match(string text)
        {
            return text.StartsWith("help");
        }

        public Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            return Task.FromResult((true, LongMessagesToUser.ToteHelpMessage().ToString()));

        }
    }
}