using System.Collections.Generic;
using System.Threading.Tasks;
using gamemaster.Db;
using MongoDB.Driver;

namespace gamemaster.Queries.Tote
{
    public class GetToteReportsQuery
    {
        private readonly MongoStore _ms;

        public GetToteReportsQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<List<ToteReport>> GetAsync(string toteId)
        {
            var list = await _ms.ToteReports.Find(a => a.ToteId == toteId)
                .SortByDescending(a => a.CreatedOn)
                .Limit(5)
                .ToListAsync();
            return list;
        }
    }
}