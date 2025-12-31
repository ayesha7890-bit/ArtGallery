using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Artist
    {
       
            [Key]
            public int ArtworkId { get; set; }

            [Required]
            [StringLength(200)]
            public string Title { get; set; }

            [Required]
            [StringLength(100)]
            public string Artists { get; set; }

            [StringLength(100)]
            public string Category { get; set; }

            [Required]
            [Column(TypeName = "decimal(18,2)")]
            public decimal Price { get; set; }

            [Required]
            [StringLength(50)]
            public string Type { get; set; }

            [Required]
            [StringLength(50)]
            public string Status { get; set; } = "Pending";
        public bool IsAuction { get; set; } = false;  // Auction ya fixed price
        public DateTime? AuctionStartTime { get; set; }
        public DateTime? AuctionEndTime { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal? StartingBid { get; set; } 




        public string? ImageUrl { get; set; }  // Nullable
        // 🚩 Foreign Key: Ye naye artist ka data purane se alag rakhega
        [Required]
        public int? UserId { get; set; } // "?" lagane se ye null allow karega
        // Navigation Property: User table ke sath link banane ke liye
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public bool IsApproved { get; set; } = false; // Add this line
        // Models/Artist.cs mein ye line add karein
        public int? WinnerUserId { get; set; }

        [ForeignKey("WinnerUserId")]
        public virtual User? Winner { get; set; }

        public virtual ICollection<Biding> Biding { get; set; } = new List<Biding>();
    }
}

