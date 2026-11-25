using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace API.Models
{
    public class Log
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string EventType { get; set; } = "INFO";

        public string? UserEmail { get; set; }

        public string? Details { get; set; }

        public string Date { get; set; } = DateTime.UtcNow.ToString("o");

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
