using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class SetAppStateCommand
    {
        private readonly MongoStore _ms;

        public SetAppStateCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task SaveStateAsync(AppState state)
        {
            await _ms.App.ReplaceOneAsync(Builders<AppState>.Filter.Eq(a => a.Id, state.Id), state,
                new ReplaceOptions {IsUpsert = true});
        }
    }
}