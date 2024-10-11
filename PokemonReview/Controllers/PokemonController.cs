using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonReview.Automapper.Dto;
using PokemonReview.Controllers.QueryObjects;

namespace PokemonReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PokemonController : Controller
    {
        private DataContext _context;
        private readonly IMapper _mapper;

        public PokemonController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //get all pokemons
        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PokemonDto>))]
        [ProducesResponseType(201, Type = typeof(IEnumerable<PokemonCategoryDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPokemons(string? pokemonName, [FromQuery] PokemonQueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //get all pokemons with categories included
            if (query.includeCategory == true)
            {
                var pokemonWithCategoriesData = await _context.Pokemon
                    .Include(p => p.PokemonCategories)
                    .ThenInclude(pc => pc.Category)
                    .Where(p => string.IsNullOrWhiteSpace(pokemonName) || p.Name.StartsWith(pokemonName))
                    .ToListAsync();
                var pokemonWithCategoriesDto = _mapper.Map<List<PokemonCategoryDto>>(pokemonWithCategoriesData);
                return Ok(pokemonWithCategoriesDto);
            }

            //get all pokemons without categories included
            var pokemons = _mapper.Map<List<PokemonDto>>(await _context.Pokemon
                .Where(p => string.IsNullOrWhiteSpace(pokemonName) || p.Name.StartsWith(pokemonName))
                .ToListAsync());
            return Ok(pokemons);
        }

        //get a single pokemon by id
        [HttpGet("{pokeId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(PokemonDto))]
        [ProducesResponseType(201, Type = typeof(PokemonCategoryDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPokemonById(int pokeId, [FromQuery] PokemonQueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //pokemon with Id not found
            if (!await _context.Pokemon.AnyAsync(p => p.Id == pokeId))
                return NotFound();

            //get a single pokemon by id with categories included
            if (query.includeCategory == true)
            {
                var pokemonWithCategoriesData = await _context.Pokemon
                    .Include(p => p.PokemonCategories)
                    .ThenInclude(pc => pc.Category)
                    .FirstOrDefaultAsync(p => p.Id == pokeId);
                var pokemonWithCategoriesDto = _mapper.Map<PokemonCategoryDto>(pokemonWithCategoriesData);
                return Ok(pokemonWithCategoriesDto);
            }

            //get a single pokemon by id without categories included
            var pokemon = _mapper.Map<PokemonDto>(await _context.Pokemon
                .FirstOrDefaultAsync(p => p.Id == pokeId));
            return Ok(pokemon);
        }
    }
}
