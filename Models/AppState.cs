using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class AppState
    {
        [BsonId] [DataMember] public string Id { get; set; }

        [DataMember] public int DbVersion { get; set; }

        [DataMember] public string CurrentLedgerPeriod { get; set; }
    }
}