using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using gamemaster.Slack;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteFinishTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly SlackApiWrapper _slack;

        public ToteFinishTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, SlackApiWrapper slack)
        {
            _getCurrentTote = getCurrentTote;
            _slack = slack;
        }

        public bool Match(string text)
        {
            return text.StartsWith("finish");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote == null)
            {
                return (false, "Чтоб завершить тотализатор, надо его сначала создать");
            }

            if (tote.State != ToteState.Started && tote.State != ToteState.Closed)
            {
                return (false, $"Завершить можно только закрытый или запущенный тотализатор, сейчас {tote.State}");
            }

            if (tote.Owner != cmd.UserId)
            {
                return (false, "Завершить можно только свой тотализатор");
            }

            await _slack.PostAsync(new BlocksMessage(LongMessagesToUser.ToteFinishButtons(tote), cmd.UserId));
            return (true, "Отправляем меню для выбора победителя");
        }
    }
}
