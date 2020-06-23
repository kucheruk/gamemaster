using System.Threading.Tasks;
using gamemaster.Db;
using gamemaster.Queries.Tote;

namespace gamemaster.Commands
{
    public class SaveToteReportPointCommand
    {
        private readonly MongoStore _ms;
        private readonly ToteReportCountQuery _count;
        
        public SaveToteReportPointCommand(MongoStore ms, ToteReportCountQuery count)
        {
            _ms = ms;
            _count = count;
        }

        public async Task SaveAsync(MessageContext msgContext, string messageId,
            string toteId)
        {
            var tr = new ToteReport( toteId, msgContext.ChannelId,messageId);
            await _ms.ToteReports.InsertOneAsync(tr);
        }
    }
}