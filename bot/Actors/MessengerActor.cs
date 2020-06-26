using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.CommandHandlers;
using gamemaster.Messages;
using gamemaster.Queries.Tote;
using gamemaster.Slack;
using Microsoft.Extensions.Logging;

namespace gamemaster.Actors
{
    public class MessengerActor: ReceiveActor
    {
        private readonly ILogger<MessengerActor> _logger;
        private readonly GetToteReportsQuery _toteReports;
        private readonly GetToteByIdQuery _getTote;
        private readonly SlackApiWrapper _slack;

        public MessengerActor( 
            ILogger<MessengerActor> logger,
            GetToteReportsQuery toteReports, SlackApiWrapper slack,
            GetToteByIdQuery getTote)
        {
            _logger = logger;
            _toteReports = toteReports;
            _slack = slack;
            _getTote = getTote;
            ReceiveAsync<MessageToChannel>(SendToSlackChannel);
            ReceiveAsync<UpdateToteReportsMessage>(UpdateToteReports);
            ReceiveAsync<ToteWinnersLoosersReportMessage>(ReportWinnersLoosersInSlack);
        }

        private async Task SendToSlackChannel(MessageToChannel msg)
        {
            await _slack.PostAsync(msg);
        }

        public static void Send(MessageToChannel msg)
        {
            Address.Tell(msg);
        }

        private async Task ReportWinnersLoosersInSlack(ToteWinnersLoosersReportMessage msg)
        {
            var reports = await _toteReports.GetAsync(msg.ToteId);
            var tote = await _getTote.GetAsync(msg.ToteId);
            var toteReport = LongMessagesToUser.ToteWinners(tote, msg);

            foreach (var cid in reports.Select(a => a.ChannelId).Distinct())
            {
                await _slack.PostAsync(new MessageToChannel(cid, toteReport));
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
        
        public static IActorRef Address { get; private set; }
        
        protected override void PreStart()
        {
            Address = Self;
            _logger.LogInformation("Messenger STARTED");

            base.PreStart();
        }
    }
}