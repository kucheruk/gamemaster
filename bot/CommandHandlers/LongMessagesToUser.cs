using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gamemaster.Actors;
using gamemaster.Messages;
using gamemaster.Models;
using SlackAPI;

namespace gamemaster.CommandHandlers
{
    public class LongMessagesToUser
    {
        private static readonly int[] Cases = {2, 0, 1, 1, 1, 2};

        public static StringBuilder ToteHelpMessage(StringBuilder sb = null)
        {
            sb ??= new StringBuilder();
            sb.AppendLine("Тотализатор это замечательный способ разбогатеть. (Кому-то, вероятно не тебе)")
                .AppendLine("Вот команды, с которыми можно сказочно разбогатеть:")
                .AppendLine("`/tote` опубликовать информацию о текущем тотализаторе (можно прямо в канал для всех)")
                .AppendLine("`/tote new :currency: Название или тема` - создаёт новый тотализатор");

            AppendAddToteOption(sb);
            AppendRemoveToteOption(sb);
            AppendStartTote(sb);
            AppendFinishTote(sb);
            AppendCancelTote(sb);

            sb
                .AppendLine("_Кстати. При завершении тотализатора организатор получает процент от ставок._")
                .AppendLine("Дерзай! Если что-то сделаешь неправильно - бот подскажет и поможет.");
            return sb;
        }

        private static StringBuilder AppendCancelTote(StringBuilder sb)
        {
            return sb
                .AppendLine("`/tote cancel` - отменить тотализатор и вернуть монеты участникам");
        }

        private static StringBuilder AppendFinishTote(StringBuilder sb)
        {
            return sb
                .AppendLine("`/tote finish` - тотализатор завершён (вопрос о выигравшем варианте придёт в личку)");
        }

        private static StringBuilder AppendStartTote(StringBuilder sb)
        {
            return sb.AppendLine(
                "`/tote start` - запустить тотализатор (не забудь его куда-нибудь опубликовать с `/tote`");
        }

        private static StringBuilder AppendRemoveToteOption(StringBuilder sb)
        {
            return sb
                .AppendLine("`/tote remove X` - удалить вариант развития событий по его номеру");
        }

        private static StringBuilder AppendAddToteOption(StringBuilder sb)
        {
            return sb
                .AppendLine(
                    "`/tote add Вариант развития событий` - добавляет вариант, на который можно ставить деньги к текущему создаваемому тотализатору");
        }

        public static List<IBlock> ToteDetails(Tote tote, StringBuilder sb = null)
        {
            sb ??= new StringBuilder();
            var desc = GetToteDescriptionMrkdwn(tote, sb);
            var blocks = new List<IBlock>();
            blocks.Add(new SectionBlock
            {
                block_id = "tote_head",
                text = new Text
                {
                    type = TextTypes.Markdown,
                    text = desc.ToString()
                }
            });
            if (tote.State == ToteState.Started)
            {
                blocks.Add(new ActionsBlock
                {
                    block_id = "tote_actions",
                    elements = new IElement[]
                    {
                        new ButtonElement
                        {
                            text = new Text
                            {
                                type = TextTypes.PlainText,
                                text = "Хочу сделать ставку!"
                            },
                            action_id = $"start_bet:{tote.Id}"
                        }
                    }
                });
            }

            return blocks;
        }

        private static StringBuilder GetToteDescriptionMrkdwn(Tote tote, StringBuilder sb)
        {
            AddLongToteDescription(tote, sb);
            if (tote.State == ToteState.Created)
            {
                AppendAddToteOption(sb);
                if (tote.Options.Length > 0)
                {
                    AppendRemoveToteOption(sb);
                }

                AppendCancelTote(sb);
            }

            return sb;
        }

        private static void AddLongToteDescription(Tote tote, StringBuilder sb)
        {
            var allBets = tote.Options.SelectMany(a => a.Bets).ToList();
            var participantsCount = allBets.Select(a => a.User).Distinct().Count();
            var betsCount = allBets.Count;

            var totalSum = allBets?.Sum(a => a.Amount);
            var total = decimal.Round(totalSum.GetValueOrDefault(), 2);
            sb
                .AppendLine($"==ТОТАЛИЗАТОР *{tote.Description}* ==")
                .AppendLine(ToteStatus(tote))
                .Append($"{participantsCount} {ParticipantsDecl(participantsCount)}, ")
                .AppendLine($"{betsCount} {Declination(betsCount, "ставка", "ставки", "ставок")}")
                .AppendLine();
            foreach (var option in tote.Options)
            {
                AddToteOption(option, tote, sb);
            }

            sb
                .AppendLine($"Всего поставлено: {total} {tote.Currency}")
                .AppendLine();
        }

        private static string ParticipantsDecl(int participantsCount)
        {
            return $"{Declination(participantsCount, "участника", "участников", "участников")}";
        }

        private static void AddToteOption(ToteOption option, Tote tote,
            StringBuilder sb)
        {
            var participantsCount = option.Bets.Select(a => a.User).Distinct().Count();
            sb.Append(
                $"*{option.Name}* ");
            if (participantsCount > 0)
            {
                var sum = option.Bets.Sum(a => a.Amount);
                sum = Decimal.Round(sum, 2);
                sb.Append(
                    $"сделано ставок на {sum} {tote.Currency} от {participantsCount} {ParticipantsDecl(participantsCount)}");
            }

            sb.AppendLine();
        }

        private static string Declination(int number, params string[] titles)
        {
            var a = number * 1m;
            var i = Convert.ToInt32(a % 10 < 5 ? a % 10 : 5);
            return titles[a % 100 > 4 && a % 100 < 20 ? 2 : Cases[i]];
        }

        private static string ToteStatus(Tote tote)
        {
            switch (tote.State)
            {
                case ToteState.Cancelled:
                    return "Отменён";
                case ToteState.Created:
                    return "Ещё не запущен";
                case ToteState.Finished:
                    return "Завершён";
                case ToteState.Started:
                    return "Открыт приём ставок!";
            }

            return "Ошибка, да";
        }

        public static StringBuilder WelcomeToTote(Tote tote, in decimal balanceAmount)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Выбран тотализатор `{tote.Description}`.")
                .AppendLine($"Монеты этого тотализатора: {tote.Currency}.");
            AddLongToteDescription(tote, sb);

            sb.AppendLine(balanceAmount > 0
                ? $"На твоих счетах есть {balanceAmount} таких монет, ставь хоть все."
                : "У тебя на счетах нет таких монет. Надо бы сначала их разыскать!");
            return sb;
        }

        public static IBlock[] ToteOptionsButtons(Tote toteValue)
        {
            return ToteOptionsWithCta(toteValue, "Выбери вариант, который ты считаешь выигрышным", "option_select");
        }

        public static IBlock[] ToteFinishButtons(Tote toteValue)
        {
            return ToteOptionsWithCta(toteValue, "Который вариант выиграл?", "finish_tote");
        }

        private static IBlock[] ToteOptionsWithCta(Tote toteValue, string cta,
            string actionIdKey)
        {
            var blocks = new List<IBlock>();
            blocks.Add(new SectionBlock
            {
                text = new Text
                {
                    text = cta,
                    type = TextTypes.Markdown
                }
            });

            var actions = new ActionsBlock();
            var elements = new List<IElement>();
            foreach (var option in toteValue.Options)
            {
                elements.Add(new ButtonElement
                {
                    action_id = $"{actionIdKey}:{toteValue.Id}:{option.Id}",
                    text = new Text
                    {
                        text = $"{option.Name}",
                        type = TextTypes.PlainText
                    }
                });
            }

            actions.elements = elements.ToArray();
            blocks.Add(actions);
            return blocks.ToArray();
        }

        public static string ToteWinners(Tote tote, ToteWinnersLoosersReportMessage msg)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Тотализатор *{tote.Description}* завершён!");
            sb.AppendLine($"Победил вариант {msg.Option}");
            sb.AppendLine($"Комиссия организатора составила {msg.OwnerPercent} {tote.Currency}");
            sb.AppendLine("Выигрыши:");
            foreach (var rAmount in msg.Rewards.OrderByDescending(a => a.Amount))
            {
                sb.AppendLine($"<@{rAmount.Account.UserId}> {rAmount.Amount} {tote.Currency}");
            }

            sb.AppendLine("Всем спасибо за участие!");
            return sb.ToString();
        }
    }
}