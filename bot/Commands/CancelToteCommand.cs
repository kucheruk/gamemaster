using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class CancelToteCommand
    {
        private readonly MongoStore _ms;

        public CancelToteCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task CancelAsync(string toteId)
        {
            var where = Builders<Tote>.Filter;
            var upd = Builders<Tote>.Update;
            await _ms.Totes.UpdateOneAsync(
                where.Eq(a => a.Id, toteId) &
                where.Ne(a => a.State, ToteState.Finished) & where.Ne(a => a.State, ToteState.Cancelled),
                upd.Set(a => a.State, ToteState.Cancelled)
                    .Set(a => a.CancelledOn, DateTime.Now));
        }
    }
}