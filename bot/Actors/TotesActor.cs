using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries.Ledger;
using gamemaster.Queries.Tote;
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
        private readonly SaveToteReportPointCommand _saveToteReportPoint;
        private readonly SlackApiWrapper _slack;

        public TotesActor(GetToteByIdQuery getTote,
            ILogger<TotesActor> logger,
            FinishToteAmountsLogicQuery rewardsLogic,
            FinishToteCommand finishTote,
            AddBetToToteCommand addBetToTote,
            SlackApiWrapper slack, SaveToteReportPointCommand saveToteReportPoint)
        {
            _getTote = getTote;
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
            _logger.LogInformation("Totes STARTED");

            base.PreStart();
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, "Error in totes actor");
            base.PreRestart(reason, message);
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
            if (tote.State != ToteState.Started && tote.State != ToteState.Closed)
            {
                MessengerActor.Send(new MessageToChannel(msg.UserId,
                    "Чот не получается завершить тотализатор. А ты случаем не жулик?"));
                return;
            }

            await _finishTote.FinishAsync(tote.Id);
            var rewards = _rewardsLogic.CalcRewards(tote, msg.OptionId);
            foreach (var reward in rewards.ProportionalReward)
            {
                LedgerActor.Address.Tell(new ValidatedTransferMessage(tote.AccountId(), reward.Account.UserId, reward.Amount,
                    tote.Currency,
                    $"Поздравляем! Тотализатор \"{tote.Description}\" завершён, вот законный выигрыш!",
                    false, $"(Тотализатор *{tote.Description}*)"));
            }

            LedgerActor.Address.Tell(new ValidatedTransferAllFundsMessage(tote.AccountId(), tote.Owner, tote.Currency,
                "Ваше вознаграждение за проведённый тотализатор!", $"(Тотализатор *{tote.Description}*)"));
            MessengerActor.Address.Tell(new UpdateToteReportsMessage(tote.Id));
            MessengerActor.Address.Tell(new ToteWinnersLoosersReportMessage(rewards, tote));
        }

        private async Task HandleToteCancel(ToteCancelledMessage msg)
        {
            var tote = await _getTote.GetAsync(msg.ToteId);
            var bets = tote.Options.SelectMany(a => a.Bets);
            foreach (var bet in bets)
            {
                LedgerActor.Address.Tell(new ValidatedTransferMessage(msg.ToteId, bet.User, bet.Amount, tote.Currency,
                    "Тотализатор отмененён, возврат ставки", false, $"(Тотализатор *{tote.Description}*)"));
            }

            MessengerActor.Address.Tell(new UpdateToteReportsMessage(tote.Id));
        }

        private async Task HandlePlaceBet(TotePlaceBetMessage msg)
        {
            var sw = new Stopwatch();
            var tote = await _getTote.GetAsync(msg.ToteId);
            if (tote.State != ToteState.Started)
            {
                MessengerActor.Send(new MessageToChannel(msg.User,
                    "В тотализатор на данном этапе невозможно сделать ставку"));
                return;
            }
            if (msg.User == tote.Owner)
            {
                MessengerActor.Send(new MessageToChannel(msg.User,
                    "Не разрешать организатору ставить на своём тотализаторе нас научил Рррроман."));
                return;
            }
            if (string.IsNullOrEmpty(msg.OptionId))
            {
                MessengerActor.Send(new MessageToChannel(msg.User,
                    "Чтоб сделать ставку нужно выбрать один вариант исхода."));
                return;
            }
            
            if (msg.Amount <= 0.01m)
            {
                MessengerActor.Send(new MessageToChannel(msg.User,
                    "Мы принимаем только ставки, начиная с нищебродского 0.01"));
                return;
            }

            
            sw.Start();
            var resp = await LedgerActor.Address.Ask<List<AccountWithAmount>>(new GetAccountBalanceRequestMessage {UserId = msg.User, Currency = tote.Currency});
            sw.Stop();
            _logger.LogInformation($"PlaceBet Took {sw.ElapsedMilliseconds}ms");
            if (resp.Count == 0 || resp[0].Amount < msg.Amount)
            {
                MessengerActor.Send(new MessageToChannel(msg.User,
                    $"Печально, но на твоём счёте нет столько {tote.Currency} :("));
                return;
            }


            await _addBetToTote.AddAsync(tote.Id, msg.OptionId, msg.User, msg.Amount);
            LedgerActor.Address.Tell(new ValidatedTransferMessage(msg.User, tote.AccountId(), msg.Amount, tote.Currency,
                "Ставка на тотализатор", true));
            MessengerActor.Send(new MessageToChannel(msg.User,
                $"Ваша ставка в количестве {tote.Currency}{msg.Amount} отправлена на счёт тотализатора!"));
            MessengerActor.Address.Tell(new UpdateToteReportsMessage(tote.Id));
        }
    }
}