using System.ComponentModel.DataAnnotations;

namespace PokemonReview.Models
{
    public class Reviews
    {
        public Guid Id { get; set; }
        [Range(1, 10)]
        public int Ratings { get; set; }
        public DateTime CreatedAt { get; set; }
        public Pokemon Pokemon{ get; set; }
        public AppUser AppUser { get; set; }
    }
}
