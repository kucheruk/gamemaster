using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace gamemaster.Models
{
    [BsonIgnoreExtraElements]
    [DataContract]
    public class PromoCode
    {
        [BsonId]
        public string Id { get; set; }
        
        public string FromUserId { get; set; }
        
        public bool Activated { get; set; }
        
        public decimal Amount { get; set; }
        
        public string Code { get; set; }
        
        public DateTime CreatedOn { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public string ToUserId { get; set; }
        public string Currency { get; set; }
    }
}