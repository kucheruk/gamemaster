using System.Threading.Tasks;

namespace gamemaster
{
    public class SetCurrentPeriodCommand
    {
        private readonly GetAppStateQuery _getState;
        private readonly SetAppStateCommand _setState;

        public SetCurrentPeriodCommand(GetAppStateQuery getState, SetAppStateCommand setState)
        {
            _getState = getState;
            _setState = setState;
        }

        public async Task SetPeriodAsync(string newPeriod)
        {
            var state = await _getState.GetAsync();

            if (state.CurrentLedgerPeriod != newPeriod)
            {
                state.CurrentLedgerPeriod = newPeriod;
                await _setState.SaveStateAsync(state);
            }
        }
    }
}