using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace gamemaster
{
    public class StoreJournalEntryCommand
    {
        private readonly ILogger<StoreJournalEntryCommand> _logger;
        private readonly MongoStore _ms;

        public StoreJournalEntryCommand(MongoStore ms, ILogger<StoreJournalEntryCommand> logger)
        {
            _ms = ms;
            _logger = logger;
        }

        public async Task<JournalRecord> StoreAsync(OperationDescription op, AccountWithAmount rec)
        {
            var jr = new JournalRecord
            {
                Amount = rec.Amount,
                Currency = rec.Account.Currency,
                Id = ObjectId.GenerateNewId().ToString(),
                Period = op.Period,
                UserId = rec.Account.UserId,
                OperationId = op.Id,
                CreatedOn = DateTime.Now
            };
            await _ms.Journal.InsertOneAsync(jr);
            _logger.LogInformation(
                "New Journal Record for {Operation} on {Period} for {Account} with {Amount} of {Currency}", op.Id,
                op.Period, rec.Account.UserId, rec.Amount, rec.Account.Currency);
            return jr;
        }
    }
}