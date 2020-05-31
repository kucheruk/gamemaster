using System.Threading.Tasks;
using Akka.Actor;

namespace gamemaster
{
    public class DbMaintenanceService : ReceiveActor
    {
        private readonly GetAppStateQuery _getState;
        private readonly SetAppStateCommand _setState;

        public DbMaintenanceService(GetAppStateQuery getState, SetAppStateCommand setState)
        {
            _getState = getState;
            _setState = setState;
            ReceiveAsync<MsgTick>(_ => StartAsync());
        }

        protected override void PreStart()
        {
            Self.Tell(MsgTick.Instance);
            base.PreStart();
        }

        public async Task StartAsync()
        {
            var state = await _getState.GetAsync();
            if (state == null)
            {
                await _setState.SaveStateAsync(new AppState
                {
                    Id = Constants.DefaultAppInstance,
                    DbVersion = 0,
                    CurrentLedgerPeriod = string.Empty
                });
            }

            Context.Parent.Tell(new DbMainetanceDoneMessage());
        }
    }
}