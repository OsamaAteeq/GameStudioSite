using System.ComponentModel.DataAnnotations;

namespace GameStudioSite.Models
{
    public class AdminBlogPostFormModel
    {
        public string? Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = "";

        [Required, StringLength(150), RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Use only lowercase letters, numbers and single hyphens (for example: my-first-post).")]
        public string Slug { get; set; } = "";

        [Required, StringLength(300)]
        public string Excerpt { get; set; } = "";

        [Required]
        public string ContentHtml { get; set; } = "";

        [Required]
        public string ImageAltText { get; set; } = "";

        [Required]
        public string Category { get; set; } = "";

        // Comma-separated in the form, split into a List<string> in the controller
        public string TagsInput { get; set; } = "";

        [Required]
        [DataType(DataType.Date)]
        public DateTime PublishedDate { get; set; } = DateTime.Today;

        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }

        public bool ShowImage { get; set; } = true;
    }
}