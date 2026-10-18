using GameStudioSite.Models;
using MongoDB.Driver;

namespace GameStudioSite.Data
{
    public class GameRepository
    {
        private readonly IMongoCollection<Game> _games;

        public GameRepository(MongoDbContext context)
        {
            _games = context.Games;
        }

        public List<Game> GetAll()
        {
            return _games.Find(_ => true)
        .SortByDescending(g => g.Id)   // newest first, same ordering GetLatest already uses
        .ToList();
        }

        public List<Game> GetLatest(int count)
        {
            return _games.Find(_ => true)
                .SortByDescending(g => g.Id) // newest inserted first, ObjectId is roughly time-ordered
                .Limit(count)
                .ToList();
        }

        public Game? GetBySlug(string slug)
        {
            return _games.Find(g => g.Slug == slug).FirstOrDefault();
        }

        public Game? GetById(string id) => _games.Find(g => g.Id == id).FirstOrDefault();

        public void Create(Game game) => _games.InsertOne(game);

        public void Update(string id, Game game) =>
            _games.ReplaceOne(g => g.Id == id, game);

        public void Delete(string id) =>
            _games.DeleteOne(g => g.Id == id);
    }
}