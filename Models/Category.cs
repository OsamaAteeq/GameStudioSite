using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameStudioSite.Models
{
    public class Category
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public string Name { get; set; } = "";
        public bool IsPublic { get; set; } = true;
    }
}