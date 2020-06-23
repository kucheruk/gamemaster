using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Commands;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteRequestHandler
    {
        private readonly IEnumerable<ITextCommandHandler> _commands;

        public ToteRequestHandler(IEnumerable<ITextCommandHandler> commands)
        {
            _commands = commands;
        }


        public async Task<(bool success, string reason)> HandleToteAsync(string user, string text,
            MessageContext context, string responseUrl)
        {
            foreach (var command in _commands)
            {
                if (command.Match(text))
                {
                    return await command.Process(new SlackTextCommand(user, context, responseUrl, text));
                }
            }

            return (true, "Какая-то очень странная и подозрительная команда!");
        }
    }
}