using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorneoUniversitario.Domain.Entities
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public string Role { get; set; } = "user";

        [BsonElement("bio")]
        public string? Bio { get; set; }

        [BsonElement("profilePicUrl")]
        public string? ProfilePicUrl { get; set; }

        [BsonElement("location")]
        public Location? Location { get; set; }

        [BsonElement("reputation")]
        public Reputation? Reputation { get; set; }

        [BsonElement("termsAccepted")]
        public bool TermsAccepted { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "active";
    }

    public class Location
    {
        [BsonElement("latitude")]
        public double Latitude { get; set; }

        [BsonElement("longitude")]
        public double Longitude { get; set; }

        [BsonElement("manual")]
        public string Manual { get; set; } = string.Empty;
    }

    public class Reputation
    {
        [BsonElement("stars")]
        public double Stars { get; set; }

        [BsonElement("reviews")]
        public int Reviews { get; set; }
    }
}