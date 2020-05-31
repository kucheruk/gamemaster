using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace gamemaster.Commands
{
    public class StoreOperationCommand
    {
        private readonly ILogger<StoreOperationCommand> _logger;
        private readonly MongoStore _ms;
        private readonly StoreJournalEntryCommand _storeJournalEntry;

        public StoreOperationCommand(MongoStore ms, StoreJournalEntryCommand storeJournalEntry,
            ILogger<StoreOperationCommand> logger)
        {
            _ms = ms;
            _storeJournalEntry = storeJournalEntry;
            _logger = logger;
        }

        public async Task<OperationDescription> StoreAsync(string period, string userId,
            string description, List<AccountWithAmount> aggregated)
        {
            var operationDescription = new OperationDescription
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Description = description,
                UserId = userId,
                Period = period,
                CreatedOn = DateTime.Now
            };
            await _ms.Ops.InsertOneAsync(operationDescription);
            _logger.LogInformation("New Operation: {Period} {FromUser} {Description} {Id}", period, userId, description,
                operationDescription.Id);
            var recs = aggregated.Select(a => _storeJournalEntry.StoreAsync(operationDescription, a));
            await Task.WhenAll(recs);
            return operationDescription;
        }
    }
}