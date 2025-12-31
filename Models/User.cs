using System.ComponentModel.DataAnnotations;

namespace ArtGallery.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // Admin / Customer / Artist
        [Required]
        public string Role { get; set; }
    }
}
