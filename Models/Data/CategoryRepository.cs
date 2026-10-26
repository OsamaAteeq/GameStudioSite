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

        // spaces become hyphens, so "Dev Logs" is "Dev-Logs" in URLs
        public static string Slugify(string name) =>
            System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"\s+", "-");

        // true if another category would produce the same URL segment
        public bool SlugExists(string name)
        {
            var wanted = Slugify(Normalize(name));
            return _categories.Find(_ => true).Project(c => c.Name).ToList()
                .Any(n => string.Equals(Slugify(n), wanted, StringComparison.OrdinalIgnoreCase));
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