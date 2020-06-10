# slack gamification

Coins and totes for all

tote todo

[x] giveaway command /toss amount :coin:
    (splits coins between channel participants)

[x] create tote : /tote new :coin: Winning hackathon team
    (prints help on how to add options)
    (creates tote account)

[x] add options: /tote add Merry Buttons Team
    (prints options list with numbers + how to remove options + how to start)

[x] remove options : /tote remove 1
    (prints options list with numbers + how to add options + how to start)

[x] start tote : /tote start

[x] cancel tote (return bets) : /tote cancel

[x] finish tote : /tote finish
    (prints options numbers)
                 /tote finish 1 
    (winner selected)
    (print results to every participant)
    (transfer 1% of funds to tote creator)
    (transfer rest of funds to winners)

[x] report tote (in channel): /tote
    (prints tote info with options + button "i want in")
    (prints info to participant about tote ratios)

[x] "i want in" button:
    prints user balance in selected currency
    prints options to user in direct messages
    every options clickable (selectable)
    user can select option

[x] asks user for bet amount

[x] if amount is ok, transfer coins to tote account

[ ] auto cancel tote after 1 week, return funds, ban user who created tote

[ ] single /tote command with interactive buttons for all actions

[ ] inplace updates for tote report block

[ ] use selects where possible

[ ] home screen with current balance and tote status

[ ] clean interaction handlers

[ ] every command - message to actor, no direct replies or DB ops

[ ] use shortcut to create tote in channel

[ ] error message for /toss channel when bot is not invited
