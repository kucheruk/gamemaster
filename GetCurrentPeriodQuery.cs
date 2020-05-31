using System.Threading.Tasks;

namespace gamemaster
{
    public class GetCurrentPeriodQuery
    {
        private readonly GetAppStateQuery _getAppStateQuery;

        public GetCurrentPeriodQuery(GetAppStateQuery getAppStateQuery)
        {
            _getAppStateQuery = getAppStateQuery;
        }

        public async Task<string> GetPeriodAsync()
        {
            var @as = await _getAppStateQuery.GetAsync();
            return @as?.CurrentLedgerPeriod;
        }
    }
}