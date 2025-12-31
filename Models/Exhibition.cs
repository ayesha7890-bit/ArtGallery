using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Exhibition
    {
        [Key]
        public int ExhibitionId { get; set; }
        [Required, StringLength(200)]
        public string Title { get; set; }
        [Required, StringLength(500)]
        public string Venue { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required, DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [StringLength(255)]
        public string? BannerImage { get; set; }
        public bool IsApproved { get; set; } = false;

        [StringLength(50)]
        public string ApprovalStatus { get; set; } = "Pending";


        [StringLength(1000)]
        public string Description { get; set; }

        [NotMapped]
        public string Status
        {
            get
            {
                DateTime today = DateTime.Today;
                if (today < StartDate) return "Upcoming";
                else if (today >= StartDate && today <= EndDate) return "Active";
                else return "Past";
            }
        }
    }
}
