namespace GameStudioSite.Models
{
    public class HomeIndexViewModel
    {
        public List<Service> Services { get; set; } = new();
        public List<Game> LatestGames { get; set; } = new();
        public List<BlogPost> LatestBlogPosts { get; set; } = new();
    }
}