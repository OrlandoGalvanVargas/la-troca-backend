using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TorneoUniversitario.Domain.Entities
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }

        [BsonElement("title")]
        public string Titulo { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Descripcion { get; set; } = string.Empty;

        [BsonElement("category")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("photoUrl")]
        public string[] FotosUrl { get; set; } = Array.Empty<string>();

        [BsonElement("location")]
        public Location? Ubicacion { get; set; }

        [BsonElement("need")]
        public string Necesidad { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreadoEn { get; set; }

        [BsonElement("updatedAt")]
        public DateTime ActualizadoEn { get; set; }

        [BsonElement("status")]
        public string Estado { get; set; } = "activo";



    }
}