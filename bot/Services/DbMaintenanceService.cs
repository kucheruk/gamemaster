using System;
using System.Threading.Tasks;
using Akka.Actor;
using gamemaster.Commands;
using gamemaster.Messages;
using gamemaster.Models;
using gamemaster.Queries;
using Microsoft.Extensions.Logging;

namespace gamemaster.Services
{
    public class DbMaintenanceService : ReceiveActor
    {
        private readonly ILogger<DbMaintenanceService> _logger;
        private readonly GetAppStateQuery _getState;
        private readonly SetAppStateCommand _setState;

        public DbMaintenanceService(ILogger<DbMaintenanceService> logger, GetAppStateQuery getState, SetAppStateCommand setState)
        {
            _logger = logger;
            _getState = getState;
            _setState = setState;
            ReceiveAsync<MsgTick>(_ => StartAsync());
        }

        protected override void PreStart()
        {
            base.PreStart();
            _logger.LogInformation("Starting db maintenace");
            Self.Tell(MsgTick.Instance);
        }


        protected override void PreRestart(Exception reason, object message)
        {
            _logger.LogError(reason, message.ToString());
            base.PreRestart(reason, message);
        }

        private async Task StartAsync()
        {
            _logger.LogInformation("Getting DB State");
            try
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

                _logger.LogInformation("done");
                Context.Parent.Tell(new DbMainetanceDoneMessage());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting state for maintenance");
                throw;
            }
        }
    }
}