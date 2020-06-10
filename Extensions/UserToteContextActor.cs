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

        public UserToteContextActor(MessageRouter router,  CurrentPeriodService cp,  GetToteByIdQuery getTote, GetUserBalanceQuery balance)
        {
            _router = router;
            _cp = cp;
            _getTote = getTote;
            _balance = balance;
            ReceiveAsync<PlaceBetStartMessage>(SetTote);
            Receive<PlaceBetMessage>(a => PlaceBet(a));
            Receive<PlaceBetSelectOptionMessage>(a => SelectNumber(a));
            Receive<ReceiveTimeout>(Stop);
        }

        private void SelectNumber(PlaceBetSelectOptionMessage msg)
        {
            var option = _toteValue.Options.FirstOrDefault(a => a.Id == msg.OptionId);
            if (option == null)
            {
                _router.ToSlackGateway(new MessageToChannel(_user,
                    $"В этот тотализаторе не найден вариант с id {msg.OptionId}"));
            }
            else
            {
                _option = option;
                _router.ToSlackGateway(new MessageToChannel(_user,
                    $"Сохранили номер выбранного тобою варианта: [{option.Number}] ({option.Name})\nТеперь напиши мне количество монет, которые готов поставить. Просто числом."));
            }
        }

        private void PlaceBet(PlaceBetMessage msg)
        {
            if (_option == null)
            {
                _router.ToSlackGateway(new MessageToChannel(_user, $"Прежде чем сделать ставку, нужно выбрать на что ты ставишь - используй одну из кнопок вариантов"));
            }
            else
            {
                if (decimal.TryParse(msg.Text, out var amount))
                {
                    _router.LedgerPlaceBet(new TotePlaceBetMessage(_user, _tote, _option.Id,  amount));
                    Self.GracefulStop(TimeSpan.FromMilliseconds(10));
                }
                else
                {
                    _router.ToSlackGateway(new MessageToChannel(_user, $"В течении ближайших минут ждём от тебя число - количество {_toteValue.Currency}, которое ты готов поставить."));
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
            _router.ToSlackGateway(new MessageToChannel(_user, LongMessagesToUser.WelcomeToTote(_toteValue, balanceAmount).ToString()));
            _router.ToSlackGateway(new BlocksMessage(LongMessagesToUser.ToteOptionsButtons(_toteValue), _user));
        }

        private void Stop(ReceiveTimeout obj)
        {
            Self.GracefulStop(TimeSpan.FromMilliseconds(10));
        }
        
        protected override void PreStart()
        {
            Context.SetReceiveTimeout(TimeSpan.FromMinutes(5));
            base.PreStart();
        }
    }
}