using System.Threading.Tasks;
using gamemaster.Db;
using MongoDB.Driver;

namespace gamemaster.Queries
{
    public class ToteReportCountQuery
    {
        private readonly MongoStore _ms;

        public ToteReportCountQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<long> CountAsync(string toteId)
        {
            return await _ms.ToteReports.CountDocumentsAsync(Builders<ToteReport>.Filter.Eq(a => a.ToteId, toteId));
        }
    }
}