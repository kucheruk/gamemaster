using System;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class OperationDescription
    {
        public OperationDescription(string userId, string period,
            string description)
        {
            UserId = userId;
            Period = period;
            Description = description;
            Id = ObjectId.GenerateNewId().ToString();
            CreatedOn = DateTime.Now;
        }

        [BsonId] [DataMember] public string Id { get; set; }

        [DataMember] public DateTime CreatedOn { get; set; }

        [DataMember] public string UserId { get; set; }

        [DataMember] public string Period { get; set; }

        [DataMember] public string Description { get; set; }
    }
}