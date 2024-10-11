using PokemonReview.Models;

namespace PokemonReview.Automapper.Dto
{
    public class PokemonCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<CategoryDto> Category { get; set; }

    }
}
