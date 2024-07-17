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
    public class CategoryController : Controller
    {
        private DataContext _context;
        private readonly IMapper _mapper;

        public CategoryController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //get all categories
        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Category>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCategories([FromQuery] CategoryQueryObject query)
        {
            var categories = await _context.Categories
                .Where(c => string.IsNullOrWhiteSpace(query.CategoryName) || c.Name.StartsWith(query.CategoryName))//allow to filter by name
                .Select(c => _mapper.Map<CategoryDto>(c))
                .ToListAsync();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(categories);
        }

        //get a single category by id
        [HttpGet("{categoryId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(Category))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                return NotFound();

            var category = _mapper.Map<CategoryDto>(await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId));

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(category);
        }

        //get pokemons by category id
        [HttpGet("pokemon/{categoryId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Pokemon>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPokemonByCategoryId(int categoryId)
        {
            var pokemons = _mapper.Map<List<PokemonDto>>(
                await _context.PokemonCategories.Where(p => p.CategoryId == categoryId).Select(p => p.Pokemon).ToListAsync());

            if (!ModelState.IsValid)
                return BadRequest();

            return Ok(pokemons);
        }
    }
}
