using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries;
using gamemaster.Services;

namespace gamemaster.Actors
{
    public class LedgerActor : ReceiveActor
    {
        private readonly CurrentPeriodService _currentPeriod;
        private readonly EmitCurrencyCommand _emission;
        private readonly GetUserBalanceQuery _getUserBalance;
        private readonly MessageRouter _router;
        private readonly SlackResponseService _slackResponse;

        public LedgerActor(CurrentPeriodService currentPeriod,
            GetUserBalanceQuery getUserBalance,
            EmitCurrencyCommand emission,
            MessageRouter router, 
            SlackResponseService slackResponse)
        {
            _currentPeriod = currentPeriod;
            _getUserBalance = getUserBalance;
            _emission = emission;
            _router = router;
            _slackResponse = slackResponse;
            Become(Starting);
        }

        private void Starting()
        {
            ReceiveAsync<MsgTick>(Initialize);
        }

        private async Task Initialize(MsgTick arg)
        {
            await _currentPeriod.InitializeAsync();
            Become(Started);
        }

        private void Started()
        {
            ReceiveAsync<EmitMessage>(HandleEmissions);
            ReceiveAsync<GetBalanceMessage>(ResponseWithBalance);
            ReceiveAsync<MsgTick>(async a => await NewPeriod());
        }


        private async Task ResponseWithBalance(GetBalanceMessage arg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, arg.UserId);
            if (resp.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("У тебя на счету:");
                foreach (var v in resp.OrderByDescending(a => a.Amount))
                {
                    sb.AppendLine($"{v.Account.Currency}{v.Amount}");
                }

                sb.AppendLine("Вухуху, продолжай в том же духе!");
                await _slackResponse.ResponseWithText(arg.ResponseUrl, sb.ToString(), true);
            }
            else
            {
                await _slackResponse.ResponseWithText(arg.ResponseUrl, "Увы и ах, на счету пусто :cry:");
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
            Self.Tell(MsgTick.Instance);
            ScheduleNextPeriodTick(NextPeriodTimestamp());
            base.PreStart();
        }
    }
}