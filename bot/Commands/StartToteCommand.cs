using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.CommandHandlers
{
    public class StartToteCommand
    {
        private readonly MongoStore _ms;

        public StartToteCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task StartAsync(string toteId)
        {
            var where = Builders<Tote>.Filter;
            var upd = Builders<Tote>.Update;
            await _ms.Totes.UpdateOneAsync(
                where.Eq(a => a.Id, toteId) & where.Eq(a => a.State, ToteState.Created),
                upd.Set(a => a.State, ToteState.Started)
                    .Set(a => a.StartedOn, DateTime.Now));
        }
    }
}