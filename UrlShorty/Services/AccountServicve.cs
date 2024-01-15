using Microsoft.EntityFrameworkCore;
using UrlShorty.Data;
using UrlShorty.DTOs;
using UrlShorty.Exceptions;
using UrlShorty.Models;

namespace UrlShorty.Services
{
    public class AccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly MailService _mailService;
        private readonly TokenService _tokenService;

        public AccountService(ApplicationDbContext context, IConfiguration configuration, MailService mailService, TokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _mailService = mailService;
            _tokenService = tokenService;
        }

        public async Task<UserDtoWithTokens> RegisterAsync(string nickname, string email, string password, string adminCredentials)
        {
            var hashedPassword = HashPassword(password);

            var user = new ApplicationUserModel
            {
                Id = Guid.NewGuid(),
                Nickname = nickname,
                Email = email,
                Password = hashedPassword,
                IsActivated = false,
                ActivationLink = Guid.NewGuid().ToString(),
                IsAdmin = adminCredentials == "GTRR34"
            };

            _context.User.Add(user);

            await _context.SaveChangesAsync();

            var userDto = new UserDto(user);

            await _mailService.SendActivationMail(user.Email, user.ActivationLink);

            var (AccessToken, RefreshToken) = _tokenService.GenerateToken(userDto);

            await _tokenService.SaveTokenAsync(userDto.Id, RefreshToken);

            return new UserDtoWithTokens(user, AccessToken, RefreshToken);
        }

        public async Task<UserDtoWithTokens> LoginAsync(string email, string password)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    throw new ApiError(400, "User with this email not found");
                }

                // Verify the password using the VerifyPassword method
                var isPasswordValid = VerifyPassword(password, user.Password);

                if (!isPasswordValid)
                {
                    throw new ApiError(400, "Incorrect password");
                }

                var userDto = new UserDto(user);

                var tokenService = new TokenService(_context, _configuration);
                var tokens = tokenService.GenerateToken(userDto);

                await tokenService.SaveTokenAsync(userDto.Id, tokens.RefreshToken);

                return new UserDtoWithTokens(user, tokens.AccessToken, tokens.RefreshToken);
            }
            catch (Exception ex)
            {
                throw new ApiError(500, $"Login failed. {ex.Message}", ex);
            }
        }

        public async Task<string> LogoutAsync(string refreshToken)
        {
            await _tokenService.RemoveTokenAsync(refreshToken);

            return refreshToken;
        }

        public async Task<UserDtoWithTokens> RefreshAsync(string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                {
                    throw new ApiError(401, "Refresh token not provided");
                }

                var userId = _tokenService.ValidateRefreshToken(refreshToken);
                var tokenFromDb = await _tokenService.FindTokenAsync(refreshToken);

                if (tokenFromDb == null)
                {
                    throw new ApiError(401, "Invalid refresh token");
                }

                var user = await _context.User.FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    throw new ApiError(401, "User not found");
                }

                if (!user.IsActivated)
                {
                    ApiError.UserNotActivated();
                }

                var userDto = new UserDto(user);
                var tokens = _tokenService.GenerateToken(userDto);

                await _tokenService.SaveTokenAsync(userDto.Id, tokens.RefreshToken);

                return new UserDtoWithTokens(user, tokens.AccessToken, tokens.RefreshToken);
            }
            catch (Exception ex)
            {
                throw new ApiError(500, $"Refresh failed. {ex.Message}", ex);
            }
        }

        public async Task<string> ActivateAccountAsync(string activationLink)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.ActivationLink == activationLink);

                if (user == null)
                {
                    return "Incorrect activation link";
                }

                var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == user.Email && u.IsActivated);

                if (existingUser != null)
                {
                    return "User with this email already activated";
                }

                user.IsActivated = true;
                await _context.SaveChangesAsync();

                return "User successfully activated";
            }
            catch (Exception ex)
            {

                return $"Activation failed. {ex.Message}";
            }
        }

        private string HashPassword(string password)
        {
            var salt = BCrypt.Net.BCrypt.GenerateSalt();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}

