using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers.Ledger;
using gamemaster.Commands;
using gamemaster.Config;
using gamemaster.Extensions;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries.Ledger;
using gamemaster.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gamemaster.Actors
{
    public class LedgerActor : ReceiveActor
    {
        private readonly CurrentPeriodService _currentPeriod;
        private readonly EmitCurrencyCommand _emission;
        private readonly GetUserBalanceQuery _getUserBalance;
        private readonly ILogger<LedgerActor> _logger;
        private readonly SlackResponseService _slackResponse;
        private readonly TossCurrencyCommand _toss;
        private readonly IOptions<AppConfig> _app;
        private readonly IOptions<SlackConfig> _slackCfg;

        public LedgerActor(CurrentPeriodService currentPeriod,
            GetUserBalanceQuery getUserBalance,
            EmitCurrencyCommand emission,
            SlackResponseService slackResponse,
            TossCurrencyCommand toss,
            ILogger<LedgerActor> logger, IOptions<AppConfig> app,
            IOptions<SlackConfig> slackCfg)
        {
            _currentPeriod = currentPeriod;
            _getUserBalance = getUserBalance;
            _emission = emission;
            _slackResponse = slackResponse;
            _toss = toss;
            _logger = logger;
            _app = app;
            _slackCfg = slackCfg;
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
            ReceiveAsync<PromoTransferMessage>(HandlePromo);
            ReceiveAsync<GiveAwayMessage>(HandleGiveAway);
            ReceiveAsync<GetBalanceMessage>(ResponseWithBalance);
            ReceiveAsync<GetAccountBalanceRequestMessage>(GetAccountBalance);
            ReceiveAsync<ValidatedTransferMessage>(TransferToUser);
            ReceiveAsync<ValidatedTransferAllFundsMessage>(TransferAll);
            ReceiveAsync<MsgTick>(async a => await NewPeriod());
        }

        private async Task GetAccountBalance(GetAccountBalanceRequestMessage arg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, arg.UserId, arg.Currency, false);
            Context.Sender.Tell(resp);
        }

        private async Task TransferAll(ValidatedTransferAllFundsMessage msg)
        {
            var balance = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromAccount, msg.Currency, false);
            var b = balance.FirstOrDefault();
            Address.Tell(new ValidatedTransferMessage(msg.FromAccount, msg.ToAccount, b.Amount, b.Account.Currency,
                msg.OpDesc, false, msg.FromCaption));
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, "WARN: LedgerActor Restart!");
            base.PreRestart(reason, message);
        }

        private async Task HandleGiveAway(GiveAwayMessage msg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromUser, msg.Currency, false);
            var userAccount = resp.Count == 0 ? 0 : resp[0].Amount;
            var amount = msg.TossAll ? userAccount : msg.Amount;
            if (userAccount < amount || userAccount == 0)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $":cry: печально, но на счетах твоих нет столько {msg.Currency}.\nПроверить баланс можно командой /balance.",
                    true);
            }
            else
            {
                var userAmount = (amount * 1m / msg.Users.Length).Trim();
                foreach (var user in msg.Users)
                {
                    Address.Tell(new ValidatedTransferMessage(msg.FromUser, user, userAmount, msg.Currency,
                        $"Раздача монеток для участников канала <#{msg.Channel.ChannelId}>"));
                }

                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $"Запрос выполнен, {msg.Currency}{amount} отправлены пользователям канала <@{msg.Channel.ChannelId}>");
            }
        }

        private Task HandlePromo(PromoTransferMessage msg)
        {
            _logger.LogInformation($"Handle promo! {msg.Amount} {msg.FromUser} {msg.Currency} {msg.ToUser}");
            try
            {
                if (msg.Amount > 0)
                {
                    return HandlePositivePromo(msg);
                }
                else if (msg.Amount < 0)
                {
                    return HandleNegativePromo(msg);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling promo code");
                throw;
            }

            return Task.CompletedTask;
        }

        private async Task HandleNegativePromo(PromoTransferMessage msg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.ToUser, msg.Currency, false);
            var userAccount = resp.Count == 0 ? 0 : resp[0].Amount;

            var amount = Math.Abs(msg.Amount);
            if (userAccount < amount || userAccount == 0)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $"Повезло! Было бы у тебя нужное количество монет - они бы пропали :)",
                    true);
            }
            else
            {
                Address.Tell(new ValidatedTransferMessage(msg.ToUser, msg.FromUser, amount, msg.Currency,
                    "Промокоды бывают не очень-то хорошими!"));
                MessengerActor.Send(new MessageToChannel(msg.ToUser, $"Промокод промокоду рознь, прости, но этот - не самый удачный. Вжух и {amount}{msg.Currency} сгорают."));
                MessengerActor.Send(new MessageToChannel(_app.Value.AnnouncementsChannelId, $"<@{msg.ToUser}> нашёл чооорный промокод `{msg.Code}` и *теряет* {amount}{msg.Currency}"));
            }
        }

        private async Task HandlePositivePromo(PromoTransferMessage msg)
        {
            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromUser, msg.Currency, false);
            var userAccount = resp.Count == 0 ? 0 : resp[0].Amount;

            var amount = msg.Amount;
            _logger.LogInformation($"On balance for promo: {userAccount}, amount {amount} {msg.Currency}");
            if (userAccount < amount || userAccount == 0)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $":cry: печально, но в банке промокодов нет столько ({amount}) {msg.Currency}.",
                    true);
            }
            else
            {
                Address.Tell(new ValidatedTransferMessage(msg.FromUser, msg.ToUser, amount, msg.Currency,
                    $"За найденный промокод `{msg.Code}`!")); 
            }
        }

        private async Task HandleToss(TossACoinMessage msg)
        {
            if (msg.Amount <= 0 && !msg.TossAll)
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

            var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.FromUser, msg.Currency, false);
            var userAccount = resp.Count == 0 ? 0 : resp[0].Amount;
            var amount = msg.TossAll ? userAccount : msg.Amount;
            if (userAccount < amount || userAccount == 0)
            {
                await _slackResponse.ResponseWithText(msg.ResponseUrl,
                    $":cry: печально, но на счетах твоих нет столько ({amount}) {msg.Currency}.\nПроверить баланс можно командой /balance.",
                    true);
            }
            else
            {
                Address.Tell(new ValidatedTransferMessage(msg.FromUser, msg.ToUser, amount, msg.Currency,
                    msg.Comment));
                var reply = $"Запрос выполнен, {msg.Currency}{amount} отправлены пользователю <@{toUser}>";
                if (!string.IsNullOrEmpty(msg.Comment))
                {
                    reply += $" с комментарием {msg.Comment}";
                }

                MessengerActor.Send(new MessageToChannel(msg.FromUser,
                    $"Ваш перевод на {msg.Currency}{amount} выполнен. {msg.Comment}"));
                await _slackResponse.ResponseWithText(msg.ResponseUrl, reply);
            }
        }

        private async Task TransferToUser(ValidatedTransferMessage msg)
        {
            _logger.LogInformation("Got Transfer {command}", JsonConvert.SerializeObject(msg));
            try
            {
                await _toss.TransferAsync(_currentPeriod.Period, msg.FromAccount, msg.ToAccount, msg.Amount, msg.Currency);
                if (!msg.ToServiceAccount)
                {
                    var fromUserCaption = msg.FromUserCaption ?? $"<@{msg.FromAccount}>";
                    var ann = new StringBuilder();
                    var hoho = new StringBuilder();
                    hoho.AppendLine("Привет!")
                        .AppendLine("День перестал быть томным, лови монетки!")
                        .AppendLine($"Перевод {msg.Currency}{msg.Amount} от {fromUserCaption}.");
                    ann.AppendLine($"<@{msg.ToAccount}> получил {msg.Amount}{msg.Currency} {msg.Comment}");
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
                    MessengerActor.Send(new MessageToChannel(msg.ToAccount, hoho.ToString()));
                    if (!_slackCfg.Value.Admins.Contains(msg.ToAccount) && _slackCfg.Value.Admins.Contains(msg.FromAccount))
                    {
                        MessengerActor.Send(new MessageToChannel(_app.Value.AnnouncementsChannelId, ann.ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tossing");
                throw;
            }
        }


        private async Task ResponseWithBalance(GetBalanceMessage msg)
        {
            _logger.LogInformation("Balance Request {Admin} {User}", msg.Admin,  msg.UserId);
            if (msg.Admin)
            {
                var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.UserId, null, msg.Admin);
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
                var resp = await _getUserBalance.GetAsync(_currentPeriod.Period, msg.UserId, null, msg.Admin);
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
            sb.AppendLine($"<@{msg.UserId}>. У тебя на счету:");
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
                if (v.Account.UserId != Constants.CashAccount && v.Amount != 0 && !v.Account.UserId.StartsWith("tote_"))
                {
                    if (msg.UserId == "U033GDN1S" || !_slackCfg.Value.Admins.Contains(v.Account.UserId))
                    {
                        sb.AppendLine($"<@{v.Account.UserId}> {v.Account.Currency}{v.Amount.Trim()}");
                    }
                }
            }

            sb.AppendLine("Прекрасно! Давайте дарить друг другу монетки! Используйте */toss*\n");
            await _slackResponse.ResponseWithText(msg.ResponseUrl, sb.ToString(), true, true);
        }


        private async Task HandleEmissions(EmitMessage emitMessage)
        {
            await _emission.StoreEmissionAsync(_currentPeriod.Period, emitMessage.AdminId,
                "Эмиссия по запросу администратора",
                new AccountWithAmount(new Account(emitMessage.ToUser, emitMessage.Currency), emitMessage.Amount));
            if (!string.IsNullOrEmpty(emitMessage.ResponseUrl))
            {
                await _slackResponse.ResponseWithText(emitMessage.ResponseUrl,
                    $"Зачислено на счёт {emitMessage.Currency}{emitMessage.Amount.Trim()}");
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
            return now.Date.AddDays(1);
        }

        private void ScheduleNextPeriodTick(DateTime when)
        {
            Context.System.Scheduler.ScheduleTellOnce(when - DateTime.Now, Self, MsgTick.Instance, Self);
        }

        protected override void PreStart()
        {
            Address = Self;
            _logger.LogInformation("LEDGER STARTED");
            Self.Tell(MsgTick.Instance);
            ScheduleNextPeriodTick(NextPeriodTimestamp());
            base.PreStart();
        }

        protected override void PostRestart(Exception reason)
        {
            _logger.LogError(reason, "restart");
            base.PostRestart(reason);
        }

        public override void AroundPreRestart(Exception cause, object message)
        {
            _logger.LogError(cause, "Error in Ledger {Message}", message);
            base.AroundPreRestart(cause, message);
        }
        
        public static IActorRef Address { get; private set; }
    }
}