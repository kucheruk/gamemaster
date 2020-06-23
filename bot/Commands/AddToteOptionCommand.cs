using System;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using gamemaster.Queries.Tote;
using MongoDB.Bson;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class AddToteOptionCommand
    {
        private readonly MongoStore _ms;
        private readonly GetToteByIdQuery _get;

        public AddToteOptionCommand(MongoStore ms, GetToteByIdQuery get)
        {
            _ms = ms;
            _get = get;
        }

        public async Task<Tote> AddAsync(Tote current, string name)
        {
            var number = current.Options.Any() ? current.Options.Max(a => a.Number) : 0;

            await _ms.Totes.UpdateOneAsync(Builders<Tote>.Filter.Eq(a => a.Id, current.Id),

                Builders<Tote>.Update.Push(a => a.Options, new ToteOption
                {
                    Bets = Array.Empty<ToteBet>(),
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = name,
                    Number = number + 1
                }));
            return await _get.GetAsync(current.Id);
        }
    }
}