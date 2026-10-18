using GameStudioSite.Models;
using MongoDB.Driver;
using System.Globalization;

namespace GameStudioSite.Data
{
    public class CategoryRepository
    {
        private readonly IMongoCollection<Category> _categories;

        public CategoryRepository(MongoDbContext context)
        {
            _categories = context.Categories;
        }

        public List<Category> GetAll()
        {
            return _categories.Find(_ => true).SortBy(c => c.Name).ToList();
        }

        public bool NameExists(string name)
        {
            var normalized = Normalize(name);
            // Case-insensitive match: ^normalized$ anchors it to an exact match (not "contains"),
            // and "i" makes it ignore case entirely at the database level.
            var filter = Builders<Category>.Filter.Regex(
                c => c.Name,
                new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(normalized)}$", "i"));

            return _categories.Find(filter).Any();
        }

        public List<string> GetPrivateCategoryNames()
        {
            return _categories.Find(c => !c.IsPublic).Project(c => c.Name).ToList();
        }

        public void Create(string name, bool isPublic)
        {
            _categories.InsertOne(new Category { Name = Normalize(name), IsPublic = isPublic });
        }

        public void Delete(string id)
        {
            _categories.DeleteOne(c => c.Id == id);
        }

        // "cars", "CARS", "  cArS " all become "Cars" — one word is lowercased then
        // first-lettered; multi-word names get each word capitalized the same way.
        public static string Normalize(string name)
        {
            var trimmed = name.Trim();
            var textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(trimmed.ToLowerInvariant());
        }
    }
}