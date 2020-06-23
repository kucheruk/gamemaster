using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.CommandHandlers;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries;
using gamemaster.Services;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;

namespace gamemaster.Extensions
{
    public class UserToteContextActor : ReceiveActor
    {
        private readonly MessageRouter _router;
        private readonly CurrentPeriodService _cp;
        private readonly GetToteByIdQuery _getTote;
        private readonly GetUserBalanceQuery _balance;
        private string _user;
        private string _tote;
        private Tote _toteValue;
        private ToteOption _option;
        private readonly SlackApiWrapper _slack;
        private readonly ILogger<UserToteContextActor> _logger;

        public UserToteContextActor(MessageRouter router, 
            CurrentPeriodService cp, 
            GetToteByIdQuery getTote,
            GetUserBalanceQuery balance,
            SlackApiWrapper slack, 
            ILogger<UserToteContextActor> logger)
        {
            _router = router;
            _cp = cp;
            _getTote = getTote;
            _balance = balance;
            _slack = slack;
            _logger = logger;
            ReceiveAsync<PlaceBetStartMessage>(SetTote);
            ReceiveAsync<PlaceBetMessage>(PlaceBet);
            ReceiveAsync<PlaceBetSelectOptionMessage>(SelectNumber);
            Receive<ReceiveTimeout>(Stop);
        }

        private async Task SelectNumber(PlaceBetSelectOptionMessage msg)
        {
            var option = _toteValue.Options.FirstOrDefault(a => a.Id == msg.OptionId);
            if (option == null)
            {
                await _slack.PostAsync(new MessageToChannel(_user,
                    $"В этот тотализаторе не найден вариант с id {msg.OptionId}"));
            }
            else
            {
                _option = option;
                await _slack.PostAsync(new MessageToChannel(_user,
                    $"Сохранили номер выбранного тобою варианта: [{option.Number}] ({option.Name})\nТеперь напиши мне количество монет, которые готов поставить. Просто числом."));
            }
        }

        private async Task PlaceBet(PlaceBetMessage msg)
        {
            if (_option == null)
            {
                await _slack.PostAsync(new MessageToChannel(_user, $"Прежде чем сделать ставку, нужно выбрать на что ты ставишь - используй одну из кнопок вариантов"));
            }
            else
            {
                if (decimal.TryParse(msg.Text, out var amount))
                {
                    amount = decimal.Round(amount, 2);
                    _router.LedgerPlaceBet(new TotePlaceBetMessage(_user, _tote, _option.Id,  amount));
                    Self.GracefulStop(TimeSpan.FromMilliseconds(10));
                }
                else
                {
                    await _slack.PostAsync(new MessageToChannel(_user, $"В течении ближайших минут ждём от тебя число - количество {_toteValue.Currency}, которое ты готов поставить."));
                }
            }
        }


        private async Task SetTote(PlaceBetStartMessage pars)
        {
            _tote = pars.ToteId;
            _user = pars.UserId;
            _toteValue = await _getTote.GetAsync(pars.ToteId);
            var balance = await _balance.GetAsync(_cp.Period, _user, _toteValue.Currency);
            var balanceAmount = balance.Count > 0 ? balance[0].Amount : 0;
            await _slack.PostAsync(new MessageToChannel(_user, LongMessagesToUser.WelcomeToTote(_toteValue, balanceAmount).ToString()));
            await _slack.PostAsync(new BlocksMessage(LongMessagesToUser.ToteOptionsButtons(_toteValue), _user));
        }

        private void Stop(ReceiveTimeout obj)
        {
            Self.GracefulStop(TimeSpan.FromMilliseconds(10));
        }

        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, "Error");
            base.PreRestart(reason, message);
        }

        protected override void PreStart()
        {
            Context.SetReceiveTimeout(TimeSpan.FromMinutes(5));
            base.PreStart();
        }
    }
}