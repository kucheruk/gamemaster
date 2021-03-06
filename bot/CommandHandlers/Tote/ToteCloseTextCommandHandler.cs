using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using gamemaster.Services;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteCloseTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly CloseToteCommand _startTote;
        private readonly SlackResponseService _slackResponse;

        public ToteCloseTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, CloseToteCommand startTote,
            SlackResponseService slackResponse)
        {
            _getCurrentTote = getCurrentTote;
            _startTote = startTote;
            _slackResponse = slackResponse;
        }

        public bool Match(TextCommandFamily family, string text)
        {
            return family == TextCommandFamily.Tote && text.StartsWith("close");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote == null)
            {
                return (false,
                    "Чтобы закрыть тотализатор, нужно его сначала создать :) например: `/tote new :coin: Кто своровал суп?`");
            }

            if (tote.State != ToteState.Started && tote.State != ToteState.Closed)
            {
                return (false, "Закрыть можно только запущенный тотализатор");
            }
            
            if (tote.Options.Length > 6)
            {
                return (false, "Нам в принципе не жалко и больше исходов хранить, но ты часом не наркоман ли?");
            }

            await _startTote.CloseAsync(tote.Id);
            await _slackResponse.ResponseWithText(cmd.ResponseUrl,
                "Приём ставок закрыт! Теперь ожидай информации и закрывай тотализатор командой `/tote finish`", true);
            MessengerActor.Address.Tell(new UpdateToteReportsMessage(tote.Id));
            return (true, string.Empty);
        }
    }
}