using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Core.Interfaces;

namespace WebAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CacheController : ControllerBase
    {
        private readonly ICacheService _cacheService;

        public CacheController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCache()
        {
            try
            {
                await _cacheService.RemoveByPatternAsync("*");
                return Ok(new { message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to clear cache: {ex.Message}" });
            }
        }

        [HttpDelete("clear/{pattern}")]
        public async Task<IActionResult> ClearCacheByPattern(string pattern)
        {
            try
            {
                await _cacheService.RemoveByPatternAsync(pattern);
                return Ok(new { message = $"Cache cleared for pattern: {pattern}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to clear cache: {ex.Message}" });
            }
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetCacheInfo()
        {
            try
            {
                return Ok(new { 
                    message = "Redis cache is running",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Cache error: {ex.Message}" });
            }
        }

        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> ClearUserCache(int userId)
        {
            try
            {
                await _cacheService.RemoveByPatternAsync($"user:{userId}");
                await _cacheService.RemoveByPatternAsync($"token:{userId}");
                return Ok(new { message = $"User cache cleared for user ID: {userId}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to clear user cache: {ex.Message}" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCacheInfo(int userId)
        {
            try
            {
                var userCache = await _cacheService.GetAsync<object>($"user:{userId}");
                var tokenCache = await _cacheService.GetAsync<string>($"token:{userId}");
                
                return Ok(new { 
                    userId = userId,
                    userCached = userCache != null,
                    tokenCached = !string.IsNullOrEmpty(tokenCache),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to get user cache info: {ex.Message}" });
            }
        }
    }
}
