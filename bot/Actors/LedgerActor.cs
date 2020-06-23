using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries;
using gamemaster.Services;
using Microsoft.Extensions.Logging;

namespace gamemaster.Actors
{
    public class LedgerActor : ReceiveActor
    {
        private readonly CurrentPeriodService _currentPeriod;
        private readonly EmitCurrencyCommand _emission;
        private readonly GetUserBalanceQuery _getUserBalance;
        private readonly ILogger<LedgerActor> _logger;
        private readonly MessageRouter _router;
        private readonly SaveToteReportPointCommand _saveToteReportPoint;
        private readonly SlackApiWrapper _slack;
        private readonly SlackResponseService _slackResponse;
        private readonly TossCurrencyCommand _toss;

        public LedgerActor(CurrentPeriodService currentPeriod,
            GetUserBalanceQuery getUserBalance,
            EmitCurrencyCommand emission,
            MessageRouter router,
            SlackApiWrapper slack,
            SlackResponseService slackResponse,
            TossCurrencyCommand toss,
            ILogger<LedgerActor> logger,
            SaveToteReportPointCommand saveToteReportPoint)
        {
            _currentPeriod = currentPeriod;
            _getUserBalance = getUserBalance;
            _emission = emission;
            _router = router;
            _slack = slack;
            _slackResponse = slackResponse;
            _toss = toss;
            _logger = logger;
            _saveToteReportPoint = saveToteReportPoint;
            Become(Starting);
        }

        private void Starting()
        {
            ReceiveAsync<MsgTick>(Initialize);
        }

        private async Task Initialize(MsgTick msg)
        {
            await _currentPeriod.InitializeAsync();
            Become(Started);
        }

        private void Started()
        {
            ReceiveAsync<EmitMessage>(HandleEmissions);
            ReceiveAsync<TossACoinMessage>(HandleToss);
            ReceiveAsync<GiveAwayMessage>(HandleGiveAway);
            ReceiveAsync<GetBalanceMessage>(ResponseWithBalance);
            ReceiveAsync<GetAccountBalanceRequestMessage>(GetAccountBalance);
            ReceiveAsync<ToteStatusMessage>(CreateNewToteStatusReportInSlack);
            ReceiveAsync<ValidatedTransferMessage>(TransferToUser);
            ReceiveAsync<ValidatedTransferAllFundsMessage>(TransferAll);
            ReceiveAsync<MsgTick>(async a => await NewPeriod());
        }

        private async Task GetAccountBalance(GetAccountBalanceRequestMessage arg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, arg.UserId, arg.Currency);
            Context.Sender.Tell(resp);
        }

        private async Task TransferAll(ValidatedTransferAllFundsMessage msg)
        {
            var balance = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromAccount, msg.Currency);
            var b = balance.FirstOrDefault();
            Self.Tell(new ValidatedTransferMessage(msg.FromAccount, msg.ToAccount, b.Amount, b.Account.Currency,
                msg.OpDesc, false, msg.FromCaption));
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, "WARN: LedgerActor Restart!");
            base.PreRestart(reason, message);
        }

        private async Task HandleGiveAway(GiveAwayMessage msg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromUser, msg.Currency);
            if (resp.Count == 0 || resp[0].Amount < msg.Amount)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $":cry: печально, но на счетах твоих нет столько {msg.Currency}.\nПроверить баланс можно командой /balance.",
                    true);
            }
            else
            {
                var amount = msg.Amount * 1m / msg.Users.Length;
                foreach (var user in msg.Users)
                {
                    Self.Tell(new ValidatedTransferMessage(msg.FromUser, user, amount, msg.Currency,
                        $"Раздача монеток для участников канала <#{msg.Channel.ChannelId}>"));
                }

                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $"Запрос выполнен, {msg.Currency}{msg.Amount} отправлены пользователям канала <@{msg.Channel.ChannelId}>");
            }
        }

        private async Task HandleToss(TossACoinMessage msg)
        {
            if (msg.Amount <= 0)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    "Гоблины в растерянности и не умеют переводить такие суммы.");
                return;
            }

            var toUser = msg.ToUser;
            if (msg.FromUser == toUser)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    "Мы конечно можем взять монетки из твоего хранилища и положить их обратно... Ну хорошо. Вжуууух! Сделано!");
                return;
            }

            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromUser, msg.Currency);
            if (resp.Count == 0 || resp[0].Amount < msg.Amount)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $":cry: печально, но на счетах твоих нет столько {msg.Currency}.\nПроверить баланс можно командой /balance.",
                    true);
            }
            else
            {
                Self.Tell(new ValidatedTransferMessage(msg.FromUser, msg.ToUser, msg.Amount, msg.Currency,
                    msg.Comment));
                var reply = $"Запрос выполнен, {msg.Currency}{msg.Amount} отправлены пользователю <@{toUser}>";
                if (!string.IsNullOrEmpty(msg.Comment))
                {
                    reply += $" с комментарием {msg.Comment}";
                }

                await _slack.PostAsync(new MessageToChannel(msg.FromUser,
                    $"Ваш перевод на {msg.Currency}{msg.Amount} выполнен. {msg.Comment}"));
                await _slackResponse.ResponseWithText(msg.ResponseUrl, reply);
            }
        }

        private async Task TransferToUser(ValidatedTransferMessage msg)
        {
            await _toss.TransferAsync(_currentPeriod.Period, msg.FromAccount, msg.ToAccount, msg.Amount, msg.Currency);
            if (!msg.ToServiceAccount)
            {
                var fromUserCaption = msg.FromUserCaption ?? $"<@{msg.FromAccount}>";
                var hoho = new StringBuilder();
                hoho.AppendLine("Привет!")
                    .AppendLine("День перестал быть томным, лови монетки!")
                    .AppendLine($"Перевод {msg.Currency}{msg.Amount} от {fromUserCaption}.");

                if (!string.IsNullOrEmpty(msg.Comment))
                {
                    hoho.AppendLine("Комментарий к переводу:")
                        .Append(">")
                        .AppendLine(msg.Comment);
                }

                hoho.AppendLine()
                    .AppendLine("Что можно сделать с монетками:")
                    .AppendLine("`/balance` - посмотреть свой баланс")
                    .AppendLine("`/toss` - передать другому пользователю")
                    .AppendLine("`/tote` - сказочно разбогатеть с тотализатором")
                    .AppendLine("Всё в твоих руках!");
                await _slack.PostAsync(new MessageToChannel(msg.ToAccount, hoho.ToString()));
            }
        }


        private async Task ResponseWithBalance(GetBalanceMessage msg)
        {
            if (msg.Admin)
            {
                var resp = await _getUserBalance.GetAsync(_currentPeriod.Period);
                if (resp.Count > 0)
                {
                    await FormatAndReplyWithSystemBalance(msg, resp);
                }
                else
                {
                    await _slackResponse.ResponseWithText(msg.ResponseUrl,
                        "Увы и ах, в системе пусто. Добавь монеток с помощью */emit*");
                }
            }
            else
            {
                var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.UserId);
                if (resp.Count > 0)
                {
                    await FormatAndReplyWithUserBalance(msg, resp);
                }
                else
                {
                    await _slackResponse.ResponseWithText(msg.ResponseUrl, "Увы и ах, на счету пусто :cry:");
                }
            }
        }

        private async Task FormatAndReplyWithUserBalance(GetBalanceMessage msg, List<AccountWithAmount> resp)
        {
            var sb = new StringBuilder();
            sb.AppendLine("У тебя на счету:");
            foreach (var v in resp.OrderByDescending(a => a.Amount))
            {
                if (v.Amount != 0)
                {
                    sb.AppendLine($"{v.Account.Currency}{v.Amount}");
                }
            }

            sb.AppendLine("Вухуху, продолжай в том же духе!\nПодари кому-нибудь монетку с помощью */toss*");
            await _slackResponse.ResponseWithText(msg.ResponseUrl, sb.ToString(), true, true);
        }

        private async Task FormatAndReplyWithSystemBalance(GetBalanceMessage msg, List<AccountWithAmount> resp)
        {
            var sb = new StringBuilder();
            sb.AppendLine(":tada::tada::tada: В текущий момент на всех счетах!");

            foreach (var v in resp.OrderByDescending(a => a.Amount))
            {
                if (v.Account.UserId != Constants.CashAccount && v.Amount != 0)
                {
                    if (!v.Account.UserId.StartsWith("tote_"))
                    {
                        sb.AppendLine($"<@{v.Account.UserId}> {v.Account.Currency}{v.Amount}");
                    }
                }
            }

            sb.AppendLine("Прекрасно! Давайте дарить друг другу монетки! Используйте */toss*\n");
            await _slackResponse.ResponseWithText(msg.ResponseUrl, sb.ToString(), true, true);
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

        private async Task HandleEmissions(EmitMessage emitMessage)
        {
            await _emission.StoreEmissionAsync(_currentPeriod.Period, emitMessage.AdminId,
                "Эмиссия по запросу администратора",
                new AccountWithAmount(new Account(emitMessage.ToUser, emitMessage.Currency), emitMessage.Amount));
            if (!string.IsNullOrEmpty(emitMessage.ResponseUrl))
            {
                await _slackResponse.ResponseWithText(emitMessage.ResponseUrl,
                    $"Зачислено на счёт {emitMessage.Currency}{emitMessage.Amount}");
            }
        }

        private async Task NewPeriod()
        {
            var p = _currentPeriod.CreatePeriodId();
            if (p != _currentPeriod.Period)
            {
                await _currentPeriod.SwitchToPeriodAsync(p);
                ScheduleNextPeriodTick(NextPeriodTimestamp());
            }
        }

        private static DateTime NextPeriodTimestamp()
        {
            var now = DateTime.Now;
            var nextPeriodTimestamp = now.Date.AddHours(now.Hour + 1);
            return nextPeriodTimestamp;
        }

        private void ScheduleNextPeriodTick(DateTime when)
        {
            Context.System.Scheduler.ScheduleTellOnce(when - DateTime.Now, Self, MsgTick.Instance, Self);
        }

        protected override void PreStart()
        {
            _router.RegisterLedger(Self);
            Address = Self;
            _logger.LogInformation("LEDGER STARTED");
            Self.Tell(MsgTick.Instance);
            ScheduleNextPeriodTick(NextPeriodTimestamp());
            base.PreStart();
        }

        public override void AroundPreRestart(Exception cause, object message)
        {
            _logger.LogError(cause, "Error in Ledger {Message}", message);
            base.AroundPreRestart(cause, message);
        }
        public static IActorRef Address { get; private set; }
    }
}