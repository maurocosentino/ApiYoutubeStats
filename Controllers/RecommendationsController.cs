// ✅ RecommendationsController.cs
using ApiYoutubeStats.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiYoutubeStats.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _service;

        public RecommendationsController(IRecommendationService service)
        {
            _service = service;
        }

        [HttpGet("smart")]
        public async Task<IActionResult> GetSmartRecommendations([FromQuery] int limit = 10)
        {
            var result = await _service.GetSmartRecommendationsAsync(limit);
            return Ok(result);
        }

        [HttpGet("smart-by-tags")]
        public async Task<IActionResult> GetSmartRecommendationsByTags([FromQuery] int limit = 10)
        {
            var result = await _service.GetSmartRecommendationsByTagsAsync(limit);
            return Ok(result);
        }
    }
}
