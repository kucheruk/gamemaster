using System;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Actors;
using gamemaster.CommandHandlers;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries.Ledger;
using gamemaster.Queries.Tote;
using gamemaster.Services;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;

namespace gamemaster.Extensions
{
    public class UserToteContextActor : ReceiveActor
    {
        private readonly CurrentPeriodService _cp;
        private readonly GetToteByIdQuery _getTote;
        private readonly GetUserBalanceQuery _balance;
        private string _user;
        private Tote _toteValue;
        private readonly SlackApiWrapper _slack;
        private readonly ILogger<UserToteContextActor> _logger;

        public UserToteContextActor( 
            CurrentPeriodService cp, 
            GetToteByIdQuery getTote,
            GetUserBalanceQuery balance,
            SlackApiWrapper slack, 
            ILogger<UserToteContextActor> logger)
        {
            
            _cp = cp;
            _getTote = getTote;
            _balance = balance;
            _slack = slack;
            _logger = logger;
            ReceiveAsync<PlaceBetStartMessage>(SetTote);
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
                await _slack.PostAsync(new MessageToChannel(_user,
                    $"Сохранили номер выбранного тобою варианта: [{option.Number}] ({option.Name})\nТеперь напиши мне количество монет, которые готов поставить. Просто числом."));
            }
        }
        
        private async Task SetTote(PlaceBetStartMessage pars)
        {
            _user = pars.UserId;
            _toteValue = await _getTote.GetAsync(pars.ToteId);
            var balance = await _balance.GetAsync(_cp.Period, _user, _toteValue.Currency);
            var balanceAmount = balance.Count > 0 ? balance[0].Amount : 0;
            await _slack.Dialog(pars.TriggerId, LongMessagesToUser.ToteDialog(_toteValue, balanceAmount));
            // await _response.ResponseWithBlocks(pars.ResponseUrl, LongMessagesToUser.ToteOptionsButtons(_toteValue, balanceAmount).ToList(), true);
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