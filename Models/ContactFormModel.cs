using System.ComponentModel.DataAnnotations;

namespace GameStudioSite.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "Please enter your first name.")]
        [StringLength(50)]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your last name.")]
        [StringLength(50)]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please choose a subject.")]
        public string Subject { get; set; } = "";

        [Required(ErrorMessage = "Please enter a message.")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message should be at least 10 characters.")]
        public string Message { get; set; } = "";

        public string FullName => $"{FirstName} {LastName}".Trim();

        public static readonly List<string> SubjectOptions = new()
        {
            "Game Ideas",
            "Information",
            "Feedback / Complaint"
        };

    }

}