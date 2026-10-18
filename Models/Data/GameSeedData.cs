namespace GameStudioSite.Models
{
    public static class GameSeedData
    {
        private static readonly List<Game> _games = new()
        {
            new Game
            {
                Title = "Model Auto Racing Online",
                Slug = "model-auto-racing-online",
                Description = "Experience the adrenaline of remote control car racing with our thrilling online multiplayer mobile game. Customize your vehicle and compete in intense races offline or challenge friends in exhilarating online competitions.",
                ImageUrl = "/images/game-racing.jpg",
                ItchIoUrl = "https://jzb4-studios.itch.io/model-auto-racing-online"
            },
            new Game
            {
                Title = "Escape Trappist",
                Slug = "escape-trappist",
                Description = "Embark on an intense journey through the alien-infested world of Trappist in this gripping first-person shooter. Battle against swarms of aggressive insects and face off against the formidable Queen to secure your escape.",
                ImageUrl = "/images/game-trappist.jpg",
                ItchIoUrl = "https://jzb4-studios.itch.io/escape-trappist"
            },
            new Game
            {
                Title = "Super Space-Stronaut",
                Slug = "space-stronaut",
                Description = "Traverse challenging terrain, battle rogue robots, and collect crystals to unlock new levels. With engaging gameplay mechanics and vibrant visuals, this thrilling 2D platformer offers an exhilarating adventure.",
                ImageUrl = "/images/game-spacestronaut.jpg",
                ItchIoUrl = "https://jzb4-studios.itch.io/space-strounaut"
            },
            new Game
            {
                Title = "Space Adventure",
                Slug = "space-adventure",
                Description = "Embark on an action-packed journey through space in a thrilling 2D space shooter. Navigate asteroid fields, engage in intense battles with enemy ships, and track down an elusive alien threat.",
                ImageUrl = "/images/game-spaceadventure.png",
                ItchIoUrl = "https://jzb4-studios.itch.io/space-adventure"
            }
        };

        public static List<Game> GetLatest(int count)
        {
            return _games.Take(count).ToList();
        }

        public static List<Game> All()
        {
            return _games.ToList();
        }
    }
}