using System;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using gamemaster.Services;

namespace gamemaster.CommandHandlers.Tote
{
    public class NewToteTextCommandHandler : ITextCommandHandler
    {
        private readonly CreateNewToteCommand _createNewTote;
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly SlackResponseService _slackResponse;

        public NewToteTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, SlackResponseService slackResponse,
            CreateNewToteCommand createNewTote)
        {
            _getCurrentTote = getCurrentTote;
            _slackResponse = slackResponse;
            _createNewTote = createNewTote;
        }


        public bool Match(string text)
        {
            return text.StartsWith("new");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote != null && (tote.State == ToteState.Created || tote.State == ToteState.Started))
            {
                return (false,
                    "Сначала нужно завершить текущий тотализатор. Нам просто слишком лениво обрабатывать много тотализаторов одновременно, сорян");
            }

            var rest = cmd.Text.Substring(3).Trim();
            var parts = rest.Split(" ");
            if (parts.Length > 1)
            {
                var currency = CommandsPartsParse.FindCurrency(parts, null);

                if (string.IsNullOrEmpty(currency))
                {
                    return (false,
                        "Не понял в какой валюте запускать тотализатор. Пример запуска: `/tote new :currency: Кого уволят первым?`, где :currency: - любой тип монеток, существующий у пользователей на руках, например :coin:.");
                }

                rest = rest.Substring(rest.IndexOf(currency, StringComparison.OrdinalIgnoreCase) + currency.Length)
                    .Trim();

                if (string.IsNullOrEmpty(rest))
                {
                    return (false,
                        "Для старта тотализатора обязательно укажи его название. Например: `/tote new :currency: Кто победит в поедании печенек на скорость?`, где :currency: - любой тип монеток, существующий у пользователей на руках.");
                }

                var newTote = await _createNewTote.CreateNewAsync(cmd.UserId, currency, rest);
                var response = LongMessagesToUser.ToteDetails(newTote);
                await _slackResponse.ResponseWithBlocks(cmd.ResponseUrl, response, false);
                return (true, string.Empty);
            }

            return (false,
                "Для старта тотализатора обязательно укажи вид монет и его название. Например: `/tote new :currency: Кто победит в поедании печенек на скорость?`, где :currency: - любой тип монеток, существующий у пользователей на руках.");
        }
    }
}