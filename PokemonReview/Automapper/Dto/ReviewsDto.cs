using PokemonReview.Models;
using System.ComponentModel.DataAnnotations;

namespace PokemonReview.Automapper.Dto
{
    public class ReviewsDto
    {
        public Guid Id { get; set; }
        [Range(1, 10)]
        public int Ratings { get; set; }
        public DateTime CreatedAt { get; set; }
        public PokemonDto Pokemon { get; set; }
        public AppUserDto AppUser { get; set; }

    }
}
