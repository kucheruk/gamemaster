using System.Collections.Generic;
using System.Linq;
using System.Text;
using gamemaster.Models;
using SlackAPI;

namespace gamemaster.CommandHandlers
{
    public class LongMessagesToUser
    {
        public static StringBuilder ToteHelpMessage(StringBuilder sb = null)
        {
            sb ??= new StringBuilder();
            sb.AppendLine("Тотализатор это замечательный способ разбогатеть. (Кому-то, вероятно не тебе)")
                .AppendLine("Вот команды, с которыми можно сказочно разбогатеть:")
                .AppendLine("`/tote` опубликовать информацию о текущем тотализаторе (можно прямо в канал для всех)")
                .AppendLine("`/tote new Название или тема` - создаёт новый тотализатор");
         
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

        public static List<IBlock> ToteDetails(Tote tote, string userId, StringBuilder sb = null)
        {
            sb ??= new StringBuilder();
            var allBets = tote.Options.SelectMany(a => a.Bets).ToList();
            var participantsCount = allBets.Select(a => a.User).Distinct().Count();
            var betsCount = allBets.Count;
            var desc =  GetToteDescriptionMrkdwn(tote, sb, participantsCount, betsCount, allBets);
            var hasBet = allBets.Any(a => a.User == userId);
            List<IBlock> blocks = new List<IBlock>();
            blocks.Add(new SectionBlock
            {
                block_id = "tote_head",
                text =new Text()
                {
                    type = TextTypes.Markdown,
                    text = desc.ToString()
                } 
            });
            if (tote.State == ToteState.Started)
            {
                var betText = hasBet ? "Хочу сделать ещё одну ставку!" : "Хочу сделать ставку!";
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
                                text = betText
                            },
                            action_id = $"start_bet:{tote.Id}"
                        }
                    }
                });
            }

            return blocks;
        }

        private static StringBuilder GetToteDescriptionMrkdwn(Tote tote, StringBuilder sb,
            int? participantsCount, int? betsCount,
            List<ToteBet> allBets)
        {
            sb.AppendLine("Детали о текущем тотализаторе")
                .Append("Статус: ")
                .AppendLine(ToteStatus(tote))
                .AppendLine($"Участников: {participantsCount}")
                .AppendLine($"Ставок: {betsCount}")
                .AppendLine($"Всего поставлено: {allBets?.Sum(a => a.Amount)} {tote.Currency}")
                .AppendLine($"==ТОТАЛИЗАТОР {tote.Description}==");
            foreach (var option in tote.Options)
            {
                AddToteOption(option, tote, sb);
            }

            sb.AppendLine();
            if (tote.State == ToteState.Created)
            {
                AppendAddToteOption(sb);
                if (tote.Options.Length > 0)
                {
                    AppendRemoveToteOption(sb);
                }
                AppendCancelTote(sb);
                if (tote.Options.Length > 1)
                {
                    AppendRemoveToteOption(sb);
                }
            }

            if (tote.State == ToteState.Started)
            {
                AppendFinishTote(sb);
                AppendCancelTote(sb);
            }

            return sb;
        }

        private static void AddToteOption(ToteOption option, Tote tote, StringBuilder sb)
        {
            var participantsCount = option.Bets.Select(a => a.User).Distinct().Count();
            sb.Append(
                $"[{option.Number}] *{option.Name}*");
            if (participantsCount > 0)
            {
                sb.Append($"Участников: {participantsCount} ставок на {option.Bets.Sum(a => a.Amount)} {tote.Currency}");
            }

            sb.AppendLine();
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
                    return "Принимаем ставки";
            }

            return "Ошибка, да";
        }

        public static IBlock[] ToteFinishMessage(Tote tote)
        {
            var blocks = new List<IBlock>();
            blocks.Add(new SectionBlock()
            {
                text = new Text()
                {
                    type = TextTypes.Markdown,
                    text = "Чтобы завершить тотализатор, выбери выигравший вариант. *Аккуратнее*, возможности поправить ошибку уже не будет! Победители ждут!"
                }
            });
            var buttons = new List<IElement>();
            foreach (var option in tote.Options.OrderBy(a => a.Number))
            {
                buttons.Add(new ButtonElement
                {
                    action_id = $"tote_finish:{tote.Id}:{option.Id}",
                    text = new Text
                    {
                        type = TextTypes.PlainText,
                        text = option.Name
                    }
                });
            }
            blocks.Add(new ActionsBlock()
            {
                elements = buttons.ToArray()
            });
            return blocks.ToArray();
        }
    }
}