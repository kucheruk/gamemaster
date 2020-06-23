using System;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using MongoDB.Bson;

namespace gamemaster.Commands
{
    public class CreateNewToteCommand
    {
        private readonly MongoStore _ms;

        public CreateNewToteCommand(MongoStore ms)
        {
            _ms = ms;
        }

        public async Task<Tote> CreateNewAsync(string user, string currency, string name)
        {
            var tote = new Tote
            {
                Currency = currency,
                Description = name,
                Id = ObjectId.GenerateNewId().ToString(),
                Options = new ToteOption[] {},
                Owner = user,
                State = ToteState.Created,
                CreatedOn = DateTime.Now,
                FinishedOn = null,
                CancelledOn = null
            };
            await _ms.Totes.InsertOneAsync(tote);
            return tote;
        }
    }
}