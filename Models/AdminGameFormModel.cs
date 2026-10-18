using System.ComponentModel.DataAnnotations;

namespace GameStudioSite.Models
{
    public class AdminGameFormModel
    {
        public string? Id { get; set; } // null when creating, set when editing

        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [Required, StringLength(100)]
        public string Slug { get; set; } = "";

        [Required, StringLength(500)]
        public string Description { get; set; } = "";

        [Required, Url]
        public string ItchIoUrl { get; set; } = "";

        public IFormFile? ImageFile { get; set; }

        // Keeps the existing image when editing and no new file is chosen
        public string? ExistingImageUrl { get; set; }
    }
}