using System;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using gamemaster.Services;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteAddOptionTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly AddToteOptionCommand _addToteOption;
        private readonly SlackResponseService _slackResponse;

        public ToteAddOptionTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, AddToteOptionCommand addToteOption,
            SlackResponseService slackResponse)
        {
            _getCurrentTote = getCurrentTote;
            _addToteOption = addToteOption;
            _slackResponse = slackResponse;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Tote && text.StartsWith("add");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote.State == ToteState.Created)
            {
                var option = cmd.Text.Substring(4).Trim();
                if (string.IsNullOrEmpty(option))
                {
                    return (false, "Формат команды: `/tote add Какой-то вариант на который можно делать ставку`");
                }

                if (tote.Options.Any(a => String.Equals(a.Name, option, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return (false, "Не получится добавить два варианта с одинаковым названием. Но спасибо за тестирование.");
                }

                var ret = await _addToteOption.AddAsync(tote, option);
                var response = LongMessagesToUser.ToteDetails(ret);
                await _slackResponse.ResponseWithBlocks(cmd.ResponseUrl, response, false);
                return (true, string.Empty);
            }

            return (false, "Добавлять варианты можно только пока тотализатор создан, но не запущен");
        }
    }
}