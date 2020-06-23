using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Models;
using gamemaster.Services;

namespace gamemaster.CommandHandlers
{
    public class ToteStartTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly StartToteCommand _startTote;
        private readonly SlackResponseService _slackResponse;

        public ToteStartTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, StartToteCommand startTote,
            SlackResponseService slackResponse)
        {
            _getCurrentTote = getCurrentTote;
            _startTote = startTote;
            _slackResponse = slackResponse;
        }

        public bool Match(string text)
        {
            return text.StartsWith("start")
                ;
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote == null)
            {
                return (false,
                    "Чтобы запустить тотализатор, нужно его сначала создать :) например: `/tote new :coin: Кто своровал суп?`");
            }

            if (tote.State != ToteState.Created)
            {
                return (false, "Запустить можно только новый созданный тотализатор");
            }

            if (tote.Options.Length < 2)
            {
                return (false,
                    "Для запуска нужно добавить хотя бы два варианта исхода для ставок, например `/tote add Президентом будет Владимир Путин.`");
            }

            if (tote.Options.Length > 50)
            {
                return (false, "Нам в принципе не жалко и болеее 50 исходов хранить, но ты часом не наркоман ли?");
            }

            await _startTote.StartAsync(tote.Id);
            await _slackResponse.ResponseWithText(cmd.ResponseUrl,
                "Тотализатор запущен. Самое время прорекламировать его в каналах! Используй `/tote`", true);
            return (true, string.Empty);
        }
    }
}