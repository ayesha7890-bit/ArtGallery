using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        // User Foreign Key
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // Artwork Foreign Key (Artist model jo aapke paas pehle se hai)
        public int ArtworkId { get; set; }
        [ForeignKey("ArtworkId")]
        public virtual Artist Artwork { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        // Doc Requirement: Admin Approval (Pending / Approved / Rejected)
        public string AdminStatus { get; set; } = "Pending";

        // Doc Requirement: Payment Status (Unpaid / Paid)
        public string PaymentStatus { get; set; } = "Unpaid";

        // Doc Requirement: Debit Card, Credit Card, Net Banking (cite: 53)
        public string PaymentMode { get; set; }

        // Doc Requirement: Free Home Delivery (Shipping Info)
        public string ShippingAddress { get; set; }
    }
}