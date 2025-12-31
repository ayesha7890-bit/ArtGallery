using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Biding
    {


      
            [Key]
            public int BidId { get; set; }

            [Required]
            public int ArtworkId { get; set; }
            [ForeignKey("ArtworkId")]
            public virtual Artist Artwork { get; set; }

            [Required]
            public int UserId { get; set; }
            [ForeignKey("UserId")]
            public virtual User User { get; set; }

            [Required]
            [Column(TypeName = "decimal(18,2)")]
            public decimal BidAmount { get; set; }

            [Required]
            public DateTime BidTime { get; set; }
        }

    }
