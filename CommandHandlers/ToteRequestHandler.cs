using System;
using System.Threading.Tasks;
using gamemaster.Config;
using gamemaster.Models;
using gamemaster.Services;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class ToteRequestHandler
    {
        private readonly AddToteOptionCommand _addToteOption;
        private readonly IOptions<SlackConfig> _cfg;
        private readonly CreateNewToteCommand _createNewTote;
        private readonly GetCurrentToteForUserQuery _getCurrentTote;
        private readonly SlackResponseService _slackResponse;

        public ToteRequestHandler(IOptions<SlackConfig> cfg,
            SlackResponseService slackResponse, CreateNewToteCommand createNewTote,
            GetCurrentToteForUserQuery getCurrentTote, AddToteOptionCommand addToteOption)
        {
            _cfg = cfg;
            _slackResponse = slackResponse;
            _createNewTote = createNewTote;
            _getCurrentTote = getCurrentTote;
            _addToteOption = addToteOption;
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
                var helpResponse = LongMessagesToUser.ToteHelpMessage().ToString();
                return (true, helpResponse);
            }

            if (string.IsNullOrEmpty(text))
            {
                var tote = await _getCurrentTote.GetAsync(user);
                var response = LongMessagesToUser.ToteDetails(tote, user);
                await _slackResponse.ResponseWithBlocks(responseUrl, response);
                return (true, string.Empty);
            }

            if (text.StartsWith("add"))
            {
                var tote = await _getCurrentTote.GetAsync(user);
                if (tote.State == ToteState.Created)
                {
                    var option = text.Substring(4).Trim();
                    if (string.IsNullOrEmpty(option))
                    {
                        return (false, "Формат команды: `/tote add Какой-то вариант на который можно делать ставку`");
                    }

                    var ret = await _addToteOption.AddAsync(tote, option);
                    var response = LongMessagesToUser.ToteDetails(ret, user);
                    await _slackResponse.ResponseWithBlocks(responseUrl, response);
                    return (true, string.Empty);
                }

                return (false, "Добавлять варианты можно только пока тотализатор создан, но не запущен");
            }     
            if (text.StartsWith("add"))
            {
                var tote = await _getCurrentTote.GetAsync(user);
                if (tote.State == ToteState.Created)
                {
                    var option = text.Substring(4).Trim();
                    if (string.IsNullOrEmpty(option))
                    {
                        return (false, "Формат команды: `/tote add Какой-то вариант на который можно делать ставку`");
                    }

                    var ret = await _addToteOption.AddAsync(tote, option);
                    var response = LongMessagesToUser.ToteDetails(ret, user);
                    await _slackResponse.ResponseWithBlocks(responseUrl, response);
                    return (true, string.Empty);
                }

                return (false, "Добавлять варианты можно только пока тотализатор создан, но не запущен");
            }

            if (text.StartsWith("new"))
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

                rest = rest.Substring(rest.IndexOf(currency, StringComparison.OrdinalIgnoreCase), currency.Length)
                    .Trim();

                if (string.IsNullOrEmpty(rest))
                {
                    return (false,
                        "Для старта тотализатора обязательно укажи его название. Например: `/tote new :currency: Кто победит в поедании печенек на скорость?`, где :currency: - любой тип монеток, существующий у пользователей на руках.");
                }

                var newTote = await _createNewTote.CreateNewAsync(user, currency, rest);
                var response = LongMessagesToUser.ToteDetails(newTote, user);
                await _slackResponse.ResponseWithBlocks(responseUrl, response);
                return (true, string.Empty);
            }

            if (text.StartsWith(""))
            {
            }

            /*
 * tote todo
 * [x] giveaway command /toss amount :coin:
 *     (splits coins between channel participants)
 * [ ] create tote : /tote new :coin: Winning hackathon team
 *     (prints help on how to add options)
 *     (creates tote account)
 * [ ] add options: /tote add Merry Buttons Team
 *     (prints options list with numbers + how to remove options + how to start)
 * [ ] remove options : /tote remove 1
 *     (prints options list with numbers + how to add options + how to start)
 * [ ] start tote : /tote start
 * [ ] cancel tote (return bets) : /tote cancel
 * [ ] finish tote : /tote finish
 *     (prints options numbers)
 *                  /tote finish 1 
 *     (winner selected)
 *     (print results to every participant)
 *     (transfer 1% of funds to tote creator)
 *     (transfer rest of funds to winners)
 * [ ] report tote (in channel): /tote
 *     (prints tote info with options + button "i want in")
 *     (prints info to participant about tote ratios)
 * [ ] "i want in" button:
 *     prints user balance in selected currency
 *     prints options to user in direct messages
 *     every options clickable (selectable)
 *     user can select option
 * [ ] asks user for bet amount
 * [ ] if amount is ok, transfer coins to tote account
 * [ ] auto cancel tote after 1 week, return funds, ban user who created tote
 * 
 *
 */

            return (true, "got!");
        }
    }
}