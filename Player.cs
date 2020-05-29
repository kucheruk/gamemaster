using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster
{
    [BsonIgnoreExtraElements]
    public class Player
    {
        [BsonId]
        public string Id { get; set; }
        public string SlackId { get; set; }
    }
}