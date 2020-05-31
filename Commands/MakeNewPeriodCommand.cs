using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Queries;
using Microsoft.Extensions.Logging;

namespace gamemaster.Commands
{
    public class MakeNewPeriodCommand
    {
        private readonly ClosePeriodCommand _close;
        private readonly GetCurrentPeriodQuery _getCurrentPeriodQuery;
        private readonly ILogger<MakeNewPeriodCommand> _logger;
        private readonly MongoStore _ms;
        private readonly OpenPeriodCommand _open;
        private readonly SetCurrentPeriodCommand _setPeriod;
        private readonly GetPeriodTotalsQuery _totalsQuery;

        public MakeNewPeriodCommand(MongoStore ms,
            GetCurrentPeriodQuery getCurrentPeriodQuery,
            GetPeriodTotalsQuery totalsQuery,
            ClosePeriodCommand close,
            OpenPeriodCommand open, ILogger<MakeNewPeriodCommand> logger,
            SetCurrentPeriodCommand setPeriod)
        {
            _ms = ms;
            _getCurrentPeriodQuery = getCurrentPeriodQuery;
            _totalsQuery = totalsQuery;
            _close = close;
            _open = open;
            _logger = logger;
            _setPeriod = setPeriod;
        }

        public async Task<string> MakeNewPeriodAsync(string nextPeriod)
        {
            using var session = await _ms.StartSessionAsync();
            try
            {
                session.StartTransaction();
                var prevPeriod = await _getCurrentPeriodQuery.GetPeriodAsync();
                var oldTotals = await _totalsQuery.GetTotalsAsync(prevPeriod);
                await _close.ClosePeriodAsync(oldTotals, prevPeriod);
                await _open.OpenPeriodAsync(oldTotals, nextPeriod);
                await _setPeriod.SetPeriodAsync(nextPeriod);
                await session.CommitTransactionAsync();
                return nextPeriod;
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Error while changing period");
            }

            return nextPeriod;
        }
    }
}