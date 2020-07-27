using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class CloseToteCommand
    {
        private readonly MongoStore _ms;
        public CloseToteCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task CloseAsync(string toteId)
        {
            var where = Builders<Tote>.Filter;
            var upd = Builders<Tote>.Update;
            await _ms.Totes.UpdateOneAsync(
                where.Eq(a => a.Id, toteId) & where.Eq(a => a.State, ToteState.Created),
                upd.Set(a => a.State, ToteState.Closed)
                    .Set(a => a.ClosedOn, DateTime.Now));
        }
    }
}