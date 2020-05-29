using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class JournalRecord
    {
        [BsonId]
        [DataMember]
        public string Id { get; set; }
        
        [DataMember]
        public DateTime CreatedOn { get; set; }
        
        [DataMember]
        public string BookId { get; set; }
        
        [DataMember]
        public string AccountId { get; set; }
        
        [DataMember]
        public string ActionId { get; set; }
        
        [DataMember]
        public decimal Debit { get; set; }
        
        [DataMember]
        public decimal Credit { get; set; }
    }
}