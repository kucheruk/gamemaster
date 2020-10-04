using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using gamemaster.Services;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteRemoveOptionTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly RemoveToteOptionCommand _removeToteOption;
        private readonly SlackResponseService _slackResponse;

        public ToteRemoveOptionTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, RemoveToteOptionCommand removeToteOption,
            SlackResponseService slackResponse)
        {
            _getCurrentTote = getCurrentTote;
            _removeToteOption = removeToteOption;
            _slackResponse = slackResponse;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Tote && text.StartsWith("remove");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote.State == ToteState.Created)
            {
                if (int.TryParse(cmd.Text.Substring(6).Trim(), out var option))
                {
                    return (false,
                        "Формат команды: `/tote remove <number>`, где number - порядковый номер варианта");
                }

                var ret = await _removeToteOption.RemoveAsync(tote, option);
                var response = LongMessagesToUser.ToteDetails(ret);
                await _slackResponse.ResponseWithBlocks(cmd.ResponseUrl, response, true);
                return (true, string.Empty);
            }

            return (false, "Удалять варианты можно только пока тотализатор создан, но не запущен");
        }
    }
}