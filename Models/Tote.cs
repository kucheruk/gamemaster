using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    public class Tote
    {
        [BsonId] public string Id { get; set; }

        public DateTime CreatedOn { get; set; }

        [BsonRepresentation(BsonType.String)] public ToteState State { get; set; }

        public string Currency { get; set; }

        public string Description { get; set; }

        public string Owner { get; set; }
        public ToteOption[] Options { get; set; }
    }
}