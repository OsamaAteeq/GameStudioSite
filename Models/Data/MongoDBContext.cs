using MongoDB.Driver;
using GameStudioSite.Models;

namespace GameStudioSite.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var connectionString = config["MongoDB:ConnectionString"];
            var databaseName = config["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public void EnsureIndexes(ILogger logger)
        {
            try
            {
                var keys = Builders<BlogPost>.IndexKeys.Ascending(p => p.Slug).Ascending(p => p.Category);
                BlogPosts.Indexes.CreateOne(new CreateIndexModel<BlogPost>(
                    keys, new CreateIndexOptions { Unique = true, Name = "slug_category_unique" }));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not create the slug/category index");
            }
        }

        public IMongoCollection<Game> Games => _database.GetCollection<Game>("Games");
        public IMongoCollection<BlogPost> BlogPosts => _database.GetCollection<BlogPost>("BlogPosts");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
    }
}