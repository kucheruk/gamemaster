using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;

namespace gamemaster.Actors
{
    public class TotesActor : ReceiveActor
    {
        private readonly AddBetToToteCommand _addBetToTote;
        private readonly FinishToteCommand _finishTote;
        private readonly GetToteByIdQuery _getTote;
        private readonly ILogger<TotesActor> _logger;
        private readonly FinishToteAmountsLogicQuery _rewardsLogic;
        private readonly MessageRouter _router;
        private readonly SaveToteReportPointCommand _saveToteReportPoint;
        private readonly SlackApiWrapper _slack;

        public TotesActor(GetToteByIdQuery getTote,
            MessageRouter router,
            ILogger<TotesActor> logger,
            FinishToteAmountsLogicQuery rewardsLogic,
            FinishToteCommand finishTote,
            AddBetToToteCommand addBetToTote,
            SlackApiWrapper slack, SaveToteReportPointCommand saveToteReportPoint)
        {
            _getTote = getTote;
            _router = router;
            _logger = logger;
            _rewardsLogic = rewardsLogic;
            _finishTote = finishTote;
            _addBetToTote = addBetToTote;
            _slack = slack;
            _saveToteReportPoint = saveToteReportPoint;
            ReceiveAsync<ToteCancelledMessage>(HandleToteCancel);
            ReceiveAsync<ToteFinishedMessage>(HandleToteFinish);
            ReceiveAsync<ToteStatusMessage>(CreateNewToteStatusReportInSlack);
            ReceiveAsync<TotePlaceBetMessage>(HandlePlaceBet);
        }

        public static IActorRef Address { get; private set; }

        protected override void PreStart()
        {
            Address = Self;
            _logger.LogInformation("LEDGER STARTED");

            base.PreStart();
        }


        private async Task CreateNewToteStatusReportInSlack(ToteStatusMessage msg)
        {
            var response = LongMessagesToUser.ToteDetails(msg.Tote);

            var mess = await _slack.PostAsync(msg.Context.ChannelId, response.ToArray());
            if (mess.ok)
            {
                await _saveToteReportPoint.SaveAsync(msg.Context, mess.ts, msg.Tote.Id);
            }
        }


        private async Task HandleToteFinish(ToteFinishedMessage msg)
        {
            var tote = await _getTote.GetAsync(msg.ToteId);
            if (tote.State != ToteState.Started)
            {
                await _slack.PostAsync(new MessageToChannel(msg.UserId,
                    "Чот не получается завершить тотализатор. А ты случаем не жулик?"));
                return;
            }

            await _finishTote.FinishAsync(tote.Id);
            var rewards = await _rewardsLogic.CalcRewards(tote, msg.OptionId);
            foreach (var reward in rewards.ProportionalReward)
            {
                Self.Tell(new ValidatedTransferMessage(tote.AccountId(), reward.Account.UserId, reward.Amount,
                    tote.Currency,
                    $"Поздравляем! Тотализатор \"{tote.Description}\" завершён, вот законный выигрыш!",
                    false, $"(Тотализатор *{tote.Description}*)"));
            }

            Self.Tell(new ValidatedTransferAllFundsMessage(tote.AccountId(), tote.Owner, tote.Currency,
                "Ваше вознаграждение за проведённый тотализатор!", $"(Тотализатор *{tote.Description}*)"));
            _router.Messenger.Tell(new UpdateToteReportsMessage(tote.Id));
            _router.Messenger.Tell(new ToteWinnersLoosersReportMessage(rewards, tote));
        }

        private async Task HandleToteCancel(ToteCancelledMessage msg)
        {
            var tote = await _getTote.GetAsync(msg.ToteId);
            var bets = tote.Options.SelectMany(a => a.Bets);
            foreach (var bet in bets)
            {
                Self.Tell(new ValidatedTransferMessage(msg.ToteId, bet.User, bet.Amount, tote.Currency,
                    "Тотализатор отмененён, возврат ставки", false, $"(Тотализатор *{tote.Description}*)"));
            }

            _router.Messenger.Tell(new UpdateToteReportsMessage(tote.Id));
        }

        private async Task HandlePlaceBet(TotePlaceBetMessage msg)
        {
            var tote = await _getTote.GetAsync(msg.ToteId);
            if (tote.State != ToteState.Started)
            {
                await _slack.PostAsync(new MessageToChannel(msg.User,
                    "В тотализатор на данном этапе невозможно сделать ставку"));
                return;
            }

            if (msg.Amount <= 0.01m)
            {
                await _slack.PostAsync(new MessageToChannel(msg.User,
                    "Мы принимаем только ставки, начиная с нищебродского 0.01"));
                return;
            }

            var resp = await LedgerActor.Address.Ask<List<AccountWithAmount>>(new GetAccountBalanceRequestMessage {UserId = msg.User, Currency = tote.Currency});
            if (resp.Count == 0 || resp[0].Amount < msg.Amount)
            {
                await _slack.PostAsync(new MessageToChannel(msg.User,
                    $"Печально, но на твоём счёте нет столько {tote.Currency} :("));
                return;
            }


            await _addBetToTote.AddAsync(tote.Id, msg.OptionId, msg.User, msg.Amount);
            Self.Tell(new ValidatedTransferMessage(msg.User, tote.AccountId(), msg.Amount, tote.Currency,
                "Ставка на тотализатор", true));
            await _slack.PostAsync(new MessageToChannel(msg.User,
                $"Ваша ставка в количестве {tote.Currency}{msg.Amount} отправлена на счёт тотализатора!"));
            _router.Messenger.Tell(new UpdateToteReportsMessage(tote.Id));
        }
    }
}