using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonReview.Automapper.Dto;
using PokemonReview.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace PokemonReview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : Controller
    {
        private DataContext _context;
        private readonly IMapper _mapper;

        public ReviewsController(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewsDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetReviews()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reviewsData = await _context.Reviews
            .Include(r => r.Pokemon)
            .Include(r => r.AppUser)
            .ToListAsync();
            var reviewsDto = _mapper.Map<List<ReviewsDto>>(reviewsData);
            return Ok(reviewsDto);
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateReview([FromQuery][Required] int pokeId, [FromQuery][Required][Range(1, 10)] int ratings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //fetch current user's email
            var emailClaim = User.FindFirst(ClaimTypes.Email);

            //check and see if review already exist
            var reviews = _context.Reviews
                .Where(c => c.Pokemon.Id == pokeId && c.AppUser.Email == emailClaim.Value)
                .FirstOrDefault();
            if (reviews != null)
            {
                ModelState.AddModelError("", "Review already exists");
                return StatusCode(422, ModelState);
            }

            //Fetch the Pokémon from the database based on pokeId
            var pokemon = await _context.Pokemon
                .FirstOrDefaultAsync(p => p.Id == pokeId);

            //fetch current user using their email
            var appUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == emailClaim.Value);

            //create a new review based on pokeId and ratings
            var newReview = new Reviews
            {
                Ratings = ratings,
                CreatedAt = DateTime.UtcNow,
                Pokemon = pokemon,
                AppUser = appUser,
            };

            //Save the new review to the database
            _context.Reviews.Add(newReview);
            var saved = await _context.SaveChangesAsync();
            if (saved <= 0)
            {
                ModelState.AddModelError("", "Something went wrong while saving");
                return StatusCode(500, ModelState);
            }

            return Ok("Review created successfully.");
        }

        [HttpGet("{guid}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(ReviewsDto))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetReviewsById(Guid guid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!await _context.Reviews.AnyAsync(p => p.Id == guid))
                return NotFound();
            var reviewsData = await _context.Reviews
                .Include(r => r.Pokemon)
                .Include(r => r.AppUser)
                .Where(r => r.Id == guid)
                .ToListAsync();
            var reviewsDto = _mapper.Map<List<ReviewsDto>>(reviewsData);
            return Ok(reviewsDto);
        }

        [HttpGet("by-pokeId/{pokeId}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewsDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetReviewsByPokemonId(int pokeId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!await _context.Reviews.AnyAsync(p => p.Pokemon.Id == pokeId))
                return NotFound();
            var reviewsData = await _context.Reviews
                .Include(r => r.Pokemon)
                .Include(r => r.AppUser)
                .Where(p => p.Pokemon.Id == pokeId)
                .ToListAsync();
            var reviewsDto = _mapper.Map<List<ReviewsDto>>(reviewsData);
            return Ok(reviewsDto);
        }

        [HttpGet("by-email/{email}")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewsDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetReviewsByEmail(string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!await _context.Reviews.AnyAsync(p => p.AppUser.Email == email))
                return NotFound();
            var reviewsData = await _context.Reviews
                .Include(r => r.Pokemon)
                .Include(r => r.AppUser)
                .Where(p => p.AppUser.Email == email)
                .ToListAsync();
            var reviewsDto = _mapper.Map<List<ReviewsDto>>(reviewsData);
            return Ok(reviewsDto);
        }

        [HttpDelete("{guid}")]
        [Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteReview(Guid guid)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (!await _context.Reviews.AnyAsync(r => r.Id == guid))
                return NotFound();

            var reviewToDelete = await _context.Reviews.Where(r => r.Id == guid).FirstOrDefaultAsync();

            _context.Remove(reviewToDelete);
            var saved = await _context.SaveChangesAsync();

            if (saved <= 0)
            {
                ModelState.AddModelError("", "Something went wrong deleting review");
                return StatusCode(500, ModelState);
            }

            return Ok("Review deleted successfully.");
        }

        [HttpDelete("by-email/{email}")]
        [Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteReviewsByEmail(string email)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find all reviews by the AppUser email
            var reviewsToDelete = await _context.Reviews
                .Where(r => r.AppUser.Email == email)
                .ToListAsync();

            if (reviewsToDelete.Count == 0)
                return NotFound($"No reviews found for user with email {email}.");

            // Remove all matching reviews
            _context.Reviews.RemoveRange(reviewsToDelete);
            var saved = await _context.SaveChangesAsync();

            if (saved <= 0)
            {
                ModelState.AddModelError("", "Something went wrong deleting reviews.");
                return StatusCode(500, ModelState);
            }

            return Ok($"Successfully deleted {reviewsToDelete.Count} review(s) for user with email {email}.");
        }

        [HttpPut("{guid}")]
        [Authorize]
        [ProducesResponseType(400)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateReview([Required] Guid guid, [FromQuery][Required][Range(1, 10)] int ratings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reviewsData = await _context.Reviews
                .Include(r => r.Pokemon)
                .Include(r => r.AppUser)
                .Where(r => r.Id == guid)
                .FirstOrDefaultAsync();

            if (reviewsData == null)
                return NotFound();

            // Update the properties of the tracked entity
            reviewsData.Ratings = ratings;
            reviewsData.CreatedAt = DateTime.UtcNow;

            var saved = await _context.SaveChangesAsync();

            if (saved <= 0)
            {
                ModelState.AddModelError("", "Something went wrong updating review");
                return StatusCode(500, ModelState);
            }

            return Ok("Review updated successfully.");
        }
    }
}
