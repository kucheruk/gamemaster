using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Db
{
    [BsonIgnoreExtraElements]
    public class ToteReport
    {
        public ToteReport(string toteId, string channelId,
            string messageTs)
        {
            ToteId = toteId;
            ChannelId = channelId;
            MessageTs = messageTs;
            CreatedOn = DateTime.Now;
            Id = ObjectId.GenerateNewId().ToString();
        }

        [BsonId]
        public string Id { get; set; }
        
        public string ToteId { get; set; }
        public DateTime CreatedOn { get; set; }
        
        public string ChannelId { get; set; }
        
        public string MessageTs { get; set; }
    }
}