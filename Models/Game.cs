using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameStudioSite.Models
{
    public class Game
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string ItchIoUrl { get; set; } = "";
    }
}