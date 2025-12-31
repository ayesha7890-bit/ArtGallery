using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGallery.Models
{
    public class Category
    {
        public int Id { get; set; } // Primary Key
        public string Name { get; set; }
        public string? Description { get; set; }

        // Self-referencing for subcategories
        public int? ParentCategoryId { get; set; }
        [ForeignKey("ParentCategoryId")]

        public Category? ParentCategory { get; set; }
        public ICollection<Category>? SubCategories { get; set; }
    }
}
