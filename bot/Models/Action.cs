using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class Action
    {
        [BsonId] [DataMember] public string Id { get; set; }
    }
}