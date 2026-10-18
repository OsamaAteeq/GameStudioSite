namespace GameStudioSite.Models
{
    public class BlogIndexViewModel
    {
        public List<BlogPost> Posts { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        public string? SelectedCategory { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}