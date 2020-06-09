using System.Linq;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.CommandHandlers
{
    public class RemoveToteOptionCommand
    {
        private readonly MongoStore _ms;
        private readonly GetToteByIdQuery _get;

        public RemoveToteOptionCommand(MongoStore ms, GetToteByIdQuery get)
        {
            _ms = ms;
            _get = get;
        }

        public async Task<Tote> RemoveAsync(Tote current, int number)
        {
            var option = current.Options.FirstOrDefault(a => a.Number == number);
            var byId = Builders<Tote>.Filter.Eq(a => a.Id, current.Id);
            var removeFromOptions = Builders<Tote>.Update.Pull(a => a.Options, option);
            await _ms.Totes.UpdateOneAsync(byId, removeFromOptions);
            return await _get.GetAsync(current.Id);
        }
    }
}