using Microsoft.AspNetCore.Mvc;
using UrlShorty.Exceptions;
using UrlShorty.Models;
using UrlShorty.Services;

namespace UrlShorty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(AccountService accountService) : ControllerBase
    {
        private readonly AccountService _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Validation error", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var result = await _accountService.RegisterAsync(model.Nickname, model.Email, model.Password, model.AdminCredentials);

                HttpContext.Response.Cookies.Append("accessToken", result.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SameSite = SameSiteMode.None,
                    Secure = false,
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            try
            {
                var userData = await _accountService.LoginAsync(model.Email, model.Password);

                Response.Cookies.Append("refreshToken", userData.RefreshToken, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    MaxAge = TimeSpan.FromDays(30),
                    HttpOnly = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                    Secure = true // Make sure your application is running in HTTPS for secure cookies
                });

                return Ok(userData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Login failed", Error = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { Message = "Refresh token not provided" });
                }

                var token = await _accountService.LogoutAsync(refreshToken);

                Response.Cookies.Delete("refreshToken");

                return Ok(token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    throw new ApiError(400, "Refresh token not provided");
                }

                var userData = await _accountService.RefreshAsync(refreshToken);

                Response.Cookies.Append("refreshToken", userData.RefreshToken, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    MaxAge = TimeSpan.FromDays(30),
                    HttpOnly = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                    Secure = true
                });

                return Ok(userData);
            }
            catch (ApiError apiError)
            {
                if (apiError.StatusCode == 401 && apiError.Message == "User not activated")
                {
                    return BadRequest(new { Message = "User not activated" });
                }
                else
                {
                    return BadRequest(new { Message = apiError.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }

        [HttpGet("activate/{activationLink}")]
        public async Task<IActionResult> ActivateAccount(string activationLink)
        {
            try
            {
                var result = await _accountService.ActivateAccountAsync(activationLink);

                if (result == "User successfully activated")
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Activation failed. {ex.Message}");
            }
        }
    }
}
