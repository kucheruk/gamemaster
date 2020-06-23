using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Messages;
using gamemaster.Queries;
using Microsoft.Extensions.Logging;

namespace gamemaster.Actors
{
    public class MessengerActor: ReceiveActor
    {
        private readonly MessageRouter _router;
        private readonly ILogger<MessengerActor> _logger;
        private readonly GetToteReportsQuery _toteReports;
        private readonly GetToteByIdQuery _getTote;
        private readonly SlackApiWrapper _slack;

        public MessengerActor(MessageRouter router, 
            ILogger<MessengerActor> logger,
            GetToteReportsQuery toteReports, SlackApiWrapper slack,
            GetToteByIdQuery getTote)
        {
            _router = router;
            _logger = logger;
            _toteReports = toteReports;
            _slack = slack;
            _getTote = getTote;
            ReceiveAsync<UpdateToteReportsMessage>(UpdateToteReports);
            ReceiveAsync<ToteWinnersLoosersReportMessage>(ReportWinnersLoosersInSlack);

        }

        private async Task ReportWinnersLoosersInSlack(ToteWinnersLoosersReportMessage msg)
        {
            var reports = await _toteReports.GetAsync(msg.ToteId);
            var tote = await _getTote.GetAsync(msg.ToteId);
            var toteReport = LongMessagesToUser.ToteWinners(tote, msg);

            foreach (var report in reports)
            {
                await _slack.PostAsync(new MessageToChannel(report.ChannelId, toteReport));
            }
        }

        private async Task UpdateToteReports(UpdateToteReportsMessage arg)
        {
            var reports = await _toteReports.GetAsync(arg.ToteId);
            var tote = await _getTote.GetAsync(arg.ToteId);
            var toteReport = LongMessagesToUser.ToteDetails(tote);

            foreach (var report in reports)
            {
                await _slack.UpdateMessage(report.ChannelId, toteReport.ToArray(), report.MessageTs);
            }
        }
        
        protected override void PreStart()
        {
            _router.RegisterMessenger(Self);
            _logger.LogInformation("LEDGER STARTED");

            base.PreStart();
        }
    }
}