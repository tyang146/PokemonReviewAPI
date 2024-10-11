namespace PokemonReview.Models
{
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<PokemonCategory> PokemonCategories { get; set; }
        public ICollection<Reviews> Reviews { get; set; }
    }
}
