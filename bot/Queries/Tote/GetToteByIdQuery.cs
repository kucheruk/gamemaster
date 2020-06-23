using System.Threading.Tasks;
using gamemaster.Db;
using MongoDB.Driver;

namespace gamemaster.Queries.Tote
{
    public class GetToteByIdQuery
    {
        private readonly MongoStore _ms;

        public GetToteByIdQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<Models.Tote> GetAsync(string id)
        {
            var tote = await _ms.Totes
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();
            return tote;
        }
    }
}