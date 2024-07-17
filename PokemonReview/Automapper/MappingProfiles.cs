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
        }
    }
}
