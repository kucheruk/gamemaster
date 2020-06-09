using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    public class ToteOption
    {
        [BsonId]
        public string Id { get; set; }
        
        public int Number { get; set; }
        
        public string Name { get; set; }
        
        public ToteBet[] Bets { get; set; }
    }
}