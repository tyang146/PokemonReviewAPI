using AutoMapper;
using PokemonReview.Automapper.Dto;
using PokemonReview.Models;

namespace PokemonReview.Automapper
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Pokemon, PokemonDto>();
            CreateMap<Category, CategoryDto>();
            CreateMap<AppUser, AppUserDto>();
            CreateMap<Reviews, ReviewsDto>();

            // Mapping from Pokemon to PokemonCategoryDto (with ICollection of categories)
            CreateMap<Pokemon, PokemonCategoryDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.PokemonCategories.Select(pc => pc.Category))); // Map ICollection<PokemonCategory> to ICollection<CategoryDto>
        }
    }
}
