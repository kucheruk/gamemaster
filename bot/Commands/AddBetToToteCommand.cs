using System;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using gamemaster.Queries;
using MongoDB.Bson;
using MongoDB.Driver;

namespace gamemaster.Commands
{
    public class AddBetToToteCommand
    {
        private readonly MongoStore _ms;
        private readonly GetToteByIdQuery _get;

        public AddBetToToteCommand(MongoStore ms, GetToteByIdQuery get)
        {
            _ms = ms;
            _get = get;
        }

        public async Task<Tote> AddAsync(string id, string optionId, string user, decimal amount)
        {
            var tote = await _get.GetAsync(id);
            var optionIdx = Array.IndexOf(tote.Options, tote.Options.FirstOrDefault(a => a.Id == optionId)); 
            
            await _ms.Totes.UpdateOneAsync(Builders<Tote>.Filter.Eq(a => a.Id, id),

                Builders<Tote>.Update.Push(a => a.Options[optionIdx].Bets, new ToteBet()
                {
                    Amount = amount,
                    Id = ObjectId.GenerateNewId().ToString(),
                    User = user
                }));
            
            return await _get.GetAsync(id);
        }

    }
}