using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class JournalRecord
    {
        [BsonId] [DataMember] public string Id { get; set; }

        [DataMember] public DateTime CreatedOn { get; set; }

        [DataMember] public string UserId { get; set; }

        [DataMember] public string OperationId { get; set; }

        [DataMember] public decimal Amount { get; set; }

        [DataMember] public string Period { get; set; }

        [DataMember] public string Currency { get; set; }

        public Account ToAccount()
        {
            return new Account(UserId, Currency);
        }
    }
}