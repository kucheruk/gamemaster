using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries
{
    public class GetAppStateQuery
    {
        private readonly MongoStore _ms;

        public GetAppStateQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<AppState> GetAsync() //string instanceId = maybe in future?
        {
            var instanceId = Constants.DefaultAppInstance;
            return await _ms.App.Find(a => a.Id == instanceId).FirstOrDefaultAsync();
        }
    }
}