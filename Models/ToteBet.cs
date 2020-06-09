using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    public class ToteBet
    {
        [BsonId]
        public string Id { get; set; }
        
        public string User { get; set; }
        
        public decimal Amount { get; set; }
    }
}