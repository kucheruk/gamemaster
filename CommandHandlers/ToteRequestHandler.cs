using System;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Actors;
using gamemaster.Config;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Services;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class ToteRequestHandler
    {
        private readonly AddToteOptionCommand _addToteOption;
        private readonly CancelToteCommand _cancelTote;
        private readonly IOptions<SlackConfig> _cfg;
        private readonly CreateNewToteCommand _createNewTote;
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly RemoveToteOptionCommand _removeToteOption;
        private readonly MessageRouter _router;
        private readonly SlackResponseService _slackResponse;
        private readonly StartToteCommand _startTote;
        private readonly SlackApiWrapper _slack;

        public ToteRequestHandler(IOptions<SlackConfig> cfg,
            SlackResponseService slackResponse,
            CreateNewToteCommand createNewTote,
            GetCurrentToteForUserQuery getCurrentTote,
            AddToteOptionCommand addToteOption,
            StartToteCommand startTote,
            RemoveToteOptionCommand removeToteOption, CancelToteCommand cancelTote,
            MessageRouter router, SlackApiWrapper slack)
        {
            _cfg = cfg;
            _slackResponse = slackResponse;
            _createNewTote = createNewTote;
            _getCurrentTote = getCurrentTote;
            _addToteOption = addToteOption;
            _startTote = startTote;
            _removeToteOption = removeToteOption;
            _cancelTote = cancelTote;
            _router = router;
            _slack = slack;
        }


        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }

        public async Task<(bool success, string reason)> HandleToteAsync(string user, string text,
            MessageContext context, string responseUrl)
        {
            if (text.StartsWith("help"))
            {
                return HandleToteHelp();
            }

            if (string.IsNullOrEmpty(text))
            {
                return await HandleToteReport(user, context, responseUrl);
            }

            if (text.StartsWith("add"))
            {
                return await HandleAddToteOption(user, text, responseUrl);
            }

            if (text.StartsWith("remove"))
            {
                return await HandleRemoveToteOption(user, text, responseUrl);
            }

            if (text.StartsWith("new"))
            {
                return await HandleToteCreation(user, text, responseUrl);
            }

            if (text.StartsWith("start"))
            {
                return await HandleToteStart(user, text, responseUrl);
            }

            if (text.StartsWith("cancel"))
            {
                return await HandleToteCancel(user, text, responseUrl);
            }

            if (text.StartsWith("finish"))
            {
                return await HandleToteFinish(user, text, responseUrl);
            }
            return (true, "got!");
        }

        private async Task<(bool success, string reason)> HandleToteFinish(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote == null)
            {
                return (false, "Чтоб завершить тотализатор, надо его сначала создать");
            }

            if (tote.State != ToteState.Started)
            {
                return (false, "Завершить можно только запущенный тотализатор");
            }

            if (tote.Owner != user)
            {
                return (false, "Завершить можно только свой тотализатор");
            }

            await _slack.PostAsync(new BlocksMessage(LongMessagesToUser.ToteFinishButtons(tote), user));
            return (true, "Отправляем меню для выбора победителя");
        }

        private async Task<(bool success, string reason)> HandleToteCancel(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
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
            _router.LedgerCancelTote(new ToteCancelledMessage(tote.Id));
            return (true, "Аукцион отменён, начинаем отправку ставок обратно");
        }

        private async Task<(bool success, string reason)> HandleToteStart(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote == null)
            {
                return (false,
                    "Чтобы запустить тотализатор, нужно его сначала создать :) например: `/tote new :coin: Кто своровал суп?`");
            }

            if (tote.State != ToteState.Created)
            {
                return (false, "Запустить можно только новый созданный тотализатор");
            }

            if (tote.Options.Length < 2)
            {
                return (false,
                    "Для запуска нужно добавить хотя бы два варианта исхода для ставок, например `/tote add Президентом будет Владимир Путин.`");
            }

            if (tote.Options.Length > 50)
            {
                return (false, "Нам в принципе не жалко и болеее 50 исходов хранить, но ты часом не наркоман ли?");
            }

            await _startTote.StartAsync(tote.Id);
            await _slackResponse.ResponseWithText(responseUrl,
                "Тотализатор запущен. Самое время прорекламировать его в каналах! Используй `/tote`", true);
            return (true, string.Empty);
        }

        private async Task<(bool success, string reason)> HandleToteCreation(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote != null && (tote.State == ToteState.Created || tote.State == ToteState.Started))
            {
                return (false,
                    "Сначала нужно завершить текущий тотализатор. Нам просто слишком лениво обрабатывать много тотализаторов одновременно, сорян");
            }

            var rest = text.Substring(4).Trim();
            var currency = CommandsPartsParse.FindCurrency(rest.Split(" "), ":coin:");
            if (string.IsNullOrEmpty(currency))
            {
                return (false,
                    "Не понял в какой валюте запускать тотализатор. Пример запуска: `/tote new :currency: Кого уволят первым?`, где :currency: - любой тип монеток, существующий у пользователей на руках.");
            }

            rest = rest.Substring(rest.IndexOf(currency, StringComparison.OrdinalIgnoreCase) + currency.Length)
                .Trim();

            if (string.IsNullOrEmpty(rest))
            {
                return (false,
                    "Для старта тотализатора обязательно укажи его название. Например: `/tote new :currency: Кто победит в поедании печенек на скорость?`, где :currency: - любой тип монеток, существующий у пользователей на руках.");
            }

            var newTote = await _createNewTote.CreateNewAsync(user, currency, rest);
            var response = LongMessagesToUser.ToteDetails(newTote);
            await _slackResponse.ResponseWithBlocks(responseUrl, response, true);
            return (true, string.Empty);
        }

        private static (bool success, string reason) HandleToteHelp()
        {
            var helpResponse = LongMessagesToUser.ToteHelpMessage().ToString();
            return (true, helpResponse);
        }

        private async Task<(bool success, string reason)> HandleToteReport(string user, MessageContext context,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote == null)
            {
                return HandleToteHelp();
            }

            if (context.Type == ChannelType.Direct)
            {
                await _slackResponse.ResponseWithBlocks(responseUrl, LongMessagesToUser.ToteDetails(tote), true);
            }
            else
            {
                _router.ToteStatus(new ToteStatusMessage(context, tote));
            }

            return (true, string.Empty);
        }

        private async Task<(bool success, string reason)> HandleAddToteOption(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote.State == ToteState.Created)
            {
                var option = text.Substring(4).Trim();
                if (string.IsNullOrEmpty(option))
                {
                    return (false, "Формат команды: `/tote add Какой-то вариант на который можно делать ставку`");
                }

                if (tote.Options.Any(a => String.Equals(a.Name, text, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return (false, "Не получится добавить два варианта с одинаковым названием. Но за попытку зачёт.");
                }

                var ret = await _addToteOption.AddAsync(tote, option);
                var response = LongMessagesToUser.ToteDetails(ret);
                await _slackResponse.ResponseWithBlocks(responseUrl, response, true);
                return (true, string.Empty);
            }

            return (false, "Добавлять варианты можно только пока тотализатор создан, но не запущен");
        }

        private async Task<(bool success, string reason)> HandleRemoveToteOption(string user, string text,
            string responseUrl)
        {
            var tote = await _getCurrentTote.GetAsync(user);
            if (tote.State == ToteState.Created)
            {
                if (int.TryParse(text.Substring(6).Trim(), out var option))
                {
                    return (false,
                        "Формат команды: `/tote remove <number>`, где number - порядковый номер варианта");
                }

                var ret = await _removeToteOption.RemoveAsync(tote, option);
                var response = LongMessagesToUser.ToteDetails(ret);
                await _slackResponse.ResponseWithBlocks(responseUrl, response, true);
                return (true, string.Empty);
            }

            return (false, "Удалять варианты можно только пока тотализатор создан, но не запущен");
        }
    }
}