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

        public IMongoCollection<Game> Games => _database.GetCollection<Game>("Games");
        public IMongoCollection<BlogPost> BlogPosts => _database.GetCollection<BlogPost>("BlogPosts");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
    }
}