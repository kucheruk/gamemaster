using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Queries.Tote;
using gamemaster.Services;

namespace gamemaster.CommandHandlers.Tote
{
    public class ToteReportTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly SlackResponseService _slackResponse;

        public ToteReportTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, SlackResponseService slackResponse)
        {
            _getCurrentTote = getCurrentTote;
            _slackResponse = slackResponse;
        }

        public bool Match(string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {
            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote == null)
            {
                return (false, "Команда /tote без параметров публикует в канале информацию о тотализаторе. Для этого тотализатор нужно сначала создать. Смотри `/tote help`.");
            }

            if (cmd.Context.Type == ChannelType.Direct)
            {
                await _slackResponse.ResponseWithBlocks(cmd.ResponseUrl, LongMessagesToUser.ToteDetails(tote), false);
            }
            else
            {
                TotesActor.Address.Tell(new ToteStatusMessage(cmd.Context, tote));
            }

            return (true, string.Empty);
        }
    }
}