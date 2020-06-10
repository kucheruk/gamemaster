using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.CommandHandlers
{
    public class FinishToteCommand
    {
        private readonly MongoStore _ms;

        public FinishToteCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task FinishAsync(string toteId)
        {
            var where = Builders<Tote>.Filter;
            var upd = Builders<Tote>.Update;
            await _ms.Totes.UpdateOneAsync(
                where.Eq(a => a.Id, toteId) & where.Eq(a => a.State, ToteState.Started),
                upd.Set(a => a.State, ToteState.Finished)
                    .Set(a => a.FinishedOn, DateTime.Now));
        }
    }
}