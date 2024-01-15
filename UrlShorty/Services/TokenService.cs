using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UrlShorty.Data;
using UrlShorty.DTOs;
using UrlShorty.Models;
using UrlShorty.Exceptions;

namespace UrlShorty.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly string _apiSecretAccess;
        private readonly string _apiSecretRefresh;

        public TokenService(ApplicationDbContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;

            _apiSecretAccess = _configuration["AppSettings:JwtSecretAccess"] ?? throw new ApiError(400, "_apiSecretAccess not specified");
            _apiSecretRefresh = _configuration["AppSettings:JwtSecretRefresh"] ?? throw new ApiError(400, "_apiSecretRefresh not specified");
        }

        public (string AccessToken, string RefreshToken) GenerateToken(UserDto payload)
        {
            if (string.IsNullOrEmpty(_apiSecretAccess) || string.IsNullOrEmpty(_apiSecretRefresh))
            {
                throw new ApiError(400, "One or both jwt secrets is not specified");
            }

            var AccessToken = GenerateJwtToken(payload, _apiSecretAccess, TimeSpan.FromMinutes(30));
            var RefreshToken = GenerateJwtToken(payload, _apiSecretRefresh, TimeSpan.FromDays(30));

            return (AccessToken, RefreshToken);
        }

        public async Task SaveTokenAsync(Guid userId, string refreshToken)
        {
            try
            {
                var existingToken = await _context.Token.SingleOrDefaultAsync(t => t.UserId == userId);

                if (existingToken != null)
                {
                    existingToken.RefreshToken = refreshToken;
                }
                else
                {
                    var newToken = new TokenModel { UserId = userId, RefreshToken = refreshToken };
               
                    _context.Token.Add(newToken);
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new ApiError(400, "User with this ID already has a token.", ex);
            }
            catch (Exception ex)
            {
                throw new ApiError(500, "An unexpected error occurred", ex);
            }
        }

        public async Task RemoveTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _context.Token.SingleOrDefaultAsync(t => t.RefreshToken == refreshToken);

                if (token != null)
                {
                    _context.Token.Remove(token);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new ApiError(500, "An unexpected error occurred", ex);
            }
        }

        public Guid ValidateAccessToken(string token)
        {
            var apiSecret = _configuration["AppSettings:JwtSecretAccess"];

            if (string.IsNullOrEmpty(apiSecret))
            {
                throw new ApiError(400, "Access token secret not configured");
            }

            return ValidateJwtToken(token, apiSecret);
        }

        public Guid ValidateRefreshToken(string token)
        {
            var apiSecret = _configuration["AppSettings:JwtSecretRefresh"];

            if (string.IsNullOrEmpty(apiSecret))
            {
                throw new ApiError(400, "Refresh token secret not configured");
            }

            return ValidateJwtToken(token, apiSecret);
        }

        public async Task<TokenModel> FindTokenAsync(string refreshToken)
        {
            try
            {
                var findToken = await _context.Token.SingleOrDefaultAsync(t => t.RefreshToken == refreshToken);

                if (findToken == null)
                {
                    throw new ApiError(400, "Token not found");
                }

                return findToken;
            }
            catch (Exception ex)
            {
                throw new ApiError(500, "An unexpected error occurred", ex);
            }
        }

        private static string GenerateJwtToken(UserDto payload, string secret, TimeSpan expirationTime)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", payload.Id.ToString()) }),
                Expires = DateTime.UtcNow.Add(expirationTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static Guid ValidateJwtToken(string token, string secret)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);

                var idClaim = claimsPrincipal.FindFirst("id");

                if (idClaim != null && Guid.TryParse(idClaim.Value, out var id))
                {
                    return id;
                }

                throw new ApiError(400, "Invalid or missing 'id' claim in the JWT token");
            }
            catch (Exception ex)
            {
                throw new ApiError(500, "An unexpected error occurred", ex);
            }
        }
    }
}
