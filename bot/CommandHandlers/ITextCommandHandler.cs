using System.Threading.Tasks;
using gamemaster.Commands;

namespace gamemaster.CommandHandlers
{
    public interface ITextCommandHandler
    {
        public bool Match(string text);
        public Task<(bool result, string response)> Process(SlackTextCommand cmd);
    }
}
