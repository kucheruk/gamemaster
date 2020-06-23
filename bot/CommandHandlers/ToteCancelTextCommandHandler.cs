using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.Models;

namespace gamemaster.CommandHandlers
{
    public class ToteCancelTextCommandHandler : ITextCommandHandler
    {
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly CancelToteCommand _cancelTote;

        public ToteCancelTextCommandHandler(GetCurrentToteForUserQuery getCurrentTote, CancelToteCommand cancelTote)
        {
            _getCurrentTote = getCurrentTote;
            _cancelTote = cancelTote;
        }

        public bool Match(string text)
        {
            return text.StartsWith("cancel");
        }

        public async Task<(bool result, string response)> Process(SlackTextCommand cmd)
        {

            var tote = await _getCurrentTote.GetAsync(cmd.UserId);
            if (tote == null)
            {
                return (false,
                    "Чтобы отменить тотализатор, нужно его сначала создать и запустить :) например: `/tote new :coin: Кто своровал суп?`");
            }

            if (tote.State == ToteState.Finished)
            {
                return (false, "Уже завершённый тотализатор отменить никак нельзя");
            }

            await _cancelTote.CancelAsync(tote.Id);
            LedgerActor.Address.Tell(new ToteCancelledMessage(tote.Id));
            return (true, "Аукцион отменён, начинаем отправку ставок обратно");
        }

    }
}