using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UrlShorty.Models;
using UrlShorty.Services;
using UrlShorty.Exceptions;

namespace UrlShorty.Controllers
{
    [Route("/url")]
    [ApiController]
    public class ShortUrlController : ControllerBase
    {
        private readonly ShortUrlService _shortUrlService;

        public ShortUrlController(ShortUrlService shortUrlService)
        {
            _shortUrlService = shortUrlService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequestModel model)
        {
            try
            {
                string slug = await _shortUrlService.ValidateAndCreateShortUrlAsync(model);

                return Ok(slug);
            }
            catch (ApiError ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("get-all")]
        public ActionResult<List<ShortUrlModel>> GetAllRecords()
        {
            try
            {
                List<ShortUrlModel> result = _shortUrlService.GetAllRecords();
                return Ok(result); // Return HTTP 200 OK with the result
            }
            catch (ApiError apiError)
            {
                return BadRequest(new { Message = apiError.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("about")]
        public async Task<IActionResult> FetchAbout()
        {
            try
            {
                var text = await _shortUrlService.FetchAboutAsync();

                return Ok(text);
            }
            catch
            {
                return BadRequest("Smth went wrong");
            }
        }

        [HttpGet("about/update")]
        public async Task<IActionResult> UpdateAbout(string text)
        {
            try
            {
                var result = await _shortUrlService.UpdateAboutAsync(text);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("~/{id}", Name = "Redirection")]
        public async Task<IActionResult> Redirection(string id)
        {
            try
            {
                var url = await _shortUrlService.RedirectionAsync(id);

                if (url != null)
                {
                    return Redirect(url);
                }

                return Redirect("/?error=Link not found");
            }
            catch (ApiError apiError)
            {
                return Redirect("/?error=Internal bad request" + apiError.Message);
            }
            catch (Exception ex)
            {
                return Redirect("/?error=Internal server error " + ex.Message);
            }
        }
    }
}
