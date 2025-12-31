using ArtGallery.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.ViewModels
{
    public class ArtistView
    {
        public int ArtworkId { get; set; } 
        [Required]
        [StringLength(200)]

        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Artists { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public bool IsAuction { get; set; } = false;  // Auction ya fixed price
        public DateTime? AuctionStartTime { get; set; }
        public DateTime? AuctionEndTime { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal? StartingBid { get; set; }  // ← decimal? instead of decimal





        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";


       

        public string? ImageUrl { get; set; }  // Nullable
                                               // Models/Artist.cs mein ye line add karein
        public int? WinnerUserId { get; set; }

        [ForeignKey("WinnerUserId")]
        public virtual User? Winner { get; set; }
        public virtual ICollection<Biding> Biding { get; set; } = new List<Biding>();
    }
}


