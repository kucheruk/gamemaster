using System;
using System.Threading.Tasks;
using gamemaster.Commands;
using gamemaster.Queries;

namespace gamemaster.Services
{
    public class CurrentPeriodService
    {
        private readonly GetCurrentPeriodQuery _getPeriod;
        private readonly MakeNewPeriodCommand _newPeriod;

        public CurrentPeriodService(GetCurrentPeriodQuery getPeriod,
            MakeNewPeriodCommand newPeriod)
        {
            _getPeriod = getPeriod;
            _newPeriod = newPeriod;
        }

        public string Period { get; private set; }

        public async Task InitializeAsync()
        {
            var period = await _getPeriod.GetPeriodAsync();
            if (period != CreatePeriodId() || period == null)
            {
                Period = await _newPeriod.MakeNewPeriodAsync(period);
            }
            else
            {
                Period = period;
            }
        }

        public async Task SwitchToPeriodAsync(string period)
        {
            if (period != Period)
            {
                Period = await _newPeriod.MakeNewPeriodAsync(CreatePeriodId());
            }
        }

        public string CreatePeriodId()
        {
            return DateTime.Now.ToString("yyyy-MM-dd-HH");
        }
    }
}