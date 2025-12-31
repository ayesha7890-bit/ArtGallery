namespace ArtGallery.Models
{
    public class Wishing
    {


     
            public int Id { get; set; }

            public string UserId { get; set; }   // logged-in user

            public int ArtworkId { get; set; }   // ya ExhibitionId

            public DateTime AddedOn { get; set; }

            public Artist Artist { get; set; } 
       

    }
}
