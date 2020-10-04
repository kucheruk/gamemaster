using System.Threading.Tasks;
using gamemaster.Commands;

namespace gamemaster.CommandHandlers
{
    public enum TextCommandFamily
    {
        Tote, Promo
    }
    
    public interface ITextCommandHandler
    {
        public bool Match(TextCommandFamily family, string text);
        public Task<(bool result, string response)> Process(SlackTextCommand cmd);
    }
}
