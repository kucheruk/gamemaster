using gamemaster.Config;
using gamemaster.Messages;
using Microsoft.Extensions.Options;

namespace gamemaster.CommandHandlers
{
    public class ToteRequestHandler
    {
        private readonly IOptions<SlackConfig> _cfg;
        private readonly MessageRouter _router;

        public ToteRequestHandler(MessageRouter router, IOptions<SlackConfig> cfg)
        {
            _router = router;
            _cfg = cfg;
        }


        private bool IsAdmin(string user)
        {
            return _cfg.Value.Admins.Contains(user);
        }

        public (bool success, string reason) HandleTote(string user, string text,
            MessageContext context, string responseUrl)
        {
            if (text.StartsWith("help"))
            {
                //print help
            }

            else if (string.IsNullOrEmpty(text))
            {
                //default: print current totes info
            }

            else if (text.StartsWith("new"))
            {
                //new tote creation
            }
            
            else if (text.StartsWith(""))
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