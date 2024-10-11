using Microsoft.AspNetCore.Identity;

namespace PokemonReview.Models
{
    public class AppUser : IdentityUser
    {
        public ICollection<Reviews> Reviews { get; set; }

    }
}
