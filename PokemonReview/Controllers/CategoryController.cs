using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonReview.Automapper.Dto;

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

        //get all categories or filter by name
        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCategories(string? CategoryName)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categories = _mapper.Map<List<CategoryDto>>(await _context.Categories
                .Where(c => string.IsNullOrWhiteSpace(CategoryName) || c.Name.StartsWith(CategoryName))
                .ToListAsync());

            return Ok(categories);
        }

        //get a single category by id
        [HttpGet("{categoryId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(CategoryDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
                return NotFound();

            var category = _mapper.Map<CategoryDto>(await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == categoryId));

            return Ok(category);
        }

        //get pokemons by category id
        [HttpGet("pokemon/{categoryId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetPokemonByCategoryId(int categoryId)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var pokemons = _mapper.Map<List<PokemonDto>>(await _context.PokemonCategories
                .Where(p => p.CategoryId == categoryId).Select(p => p.Pokemon).ToListAsync());

            return Ok(pokemons);
        }
    }
}
