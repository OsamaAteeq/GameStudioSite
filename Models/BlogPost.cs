using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameStudioSite.Models
{
    public class BlogPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";

        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Excerpt { get; set; } = "";
        public string ContentHtml { get; set; } = "";
        public string FeaturedImageUrl { get; set; } = "";
        public string ImageAltText { get; set; } = "";
        public DateTime PublishedDate { get; set; }
        public string Category { get; set; } = "";
        public List<string> Tags { get; set; } = new();
        public bool ShowImage { get; set; } = true;
    }
}