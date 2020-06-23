using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries.Tote
{
    public class GetCurrentToteForUserQuery
    {
        private readonly MongoStore _ms;

        public GetCurrentToteForUserQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<Models.Tote> GetAsync(string userId)
        {
            var tote = await _ms.Totes
                .Find(a => a.Owner == userId && a.State != ToteState.Cancelled)
                .SortByDescending(a => a.CreatedOn)
                .FirstOrDefaultAsync();
            return tote;
        }
    }
}