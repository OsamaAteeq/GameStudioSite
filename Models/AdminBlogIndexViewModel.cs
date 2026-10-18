namespace GameStudioSite.Models
{
    public class AdminBlogIndexViewModel
    {
        public List<BlogPost> Posts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public HashSet<string> PrivateCategoryNames { get; set; } = new();
        public bool OldestFirst { get; set; }
    }
}