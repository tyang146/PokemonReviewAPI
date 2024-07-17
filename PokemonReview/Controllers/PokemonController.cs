using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonReview.Automapper.Dto;
using PokemonReview.Controllers.QueryObjects;
using PokemonReview.Models;

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

        //get all pokemons with or without categories
        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        public async Task<IActionResult> GetPokemons(string? pokemonName, [FromQuery] PokemonQueryObject query)
        {
            //get all pokemons with categories included
            if (query.includeCategory == true)
            {
                var pokemonWithCategories = await _context.Pokemon
                .Include(p => p.PokemonCategories)
                .ThenInclude(pc => pc.Category)
                .Where(p => string.IsNullOrWhiteSpace(pokemonName) || p.Name.StartsWith(pokemonName))//allow to filter by pokemonName
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Categories = p.PokemonCategories.Select(pc => new CategoryDto
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name
                    }).ToList()
                })
                .ToListAsync();
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                return Ok(pokemonWithCategories);
            }

            //get all pokemons without categories included
            var pokemons = _mapper.Map<List<PokemonDto>>(await _context.Pokemon
                .Where(p => string.IsNullOrWhiteSpace(pokemonName) || p.Name.StartsWith(pokemonName))//allow to filter by pokemonName
                .ToListAsync());
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            return Ok(pokemons);
        }

        //get a single pokemon by id
        [HttpGet("{pokeId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(Pokemon))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPokemon(int pokeId, [FromQuery] PokemonQueryObject query)
        {
            //get a single pokemon by id with categories included
            if (!await _context.Pokemon.AnyAsync(p => p.Id == pokeId))
                return NotFound();

            if (query.includeCategory == true)
            {
                var pokemonWithCategories = await _context.Pokemon
                .Include(p => p.PokemonCategories)
                .ThenInclude(pc => pc.Category)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Categories = p.PokemonCategories.Select(pc => new CategoryDto
                    {
                        Id = pc.Category.Id,
                        Name = pc.Category.Name
                    }).ToList()
                })
                .FirstOrDefaultAsync(p => p.Id == pokeId);

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                return Ok(pokemonWithCategories);
            }

            //get a single pokemon by id without categories included
            var pokemon = _mapper.Map<PokemonDto>(await _context.Pokemon.FirstOrDefaultAsync(p => p.Id == pokeId));
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(pokemon);
        }
    }
}
