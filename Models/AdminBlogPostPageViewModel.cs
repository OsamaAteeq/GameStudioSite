namespace GameStudioSite.Models
{
    public class AdminBlogPostPageViewModel
    {
        public AdminBlogPostFormModel Form { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}