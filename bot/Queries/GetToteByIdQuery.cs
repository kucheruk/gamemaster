using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Queries
{
    public class GetToteByIdQuery
    {
        private readonly MongoStore _ms;

        public GetToteByIdQuery(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<Tote> GetAsync(string id)
        {
            var tote = await _ms.Totes
                .Find(a => a.Id == id)
                .FirstOrDefaultAsync();
            return tote;
        }
    }
}