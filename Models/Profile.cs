using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Profile
    {

        [Key]
        public int ProfileId { get; set; }

        // User Table ke sath relationship
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        [Required]
        public string FullName { get; set; }

        [Required]
        public int Age { get; set; } // Age [cite: 49]

        [Required]
        public string Sex { get; set; } // Sex [cite: 49]

        [Required]
        public string Interest { get; set; } // Interest related to paintings [cite: 49]
    }

}