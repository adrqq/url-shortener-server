using UrlShorty.Models;

namespace UrlShorty.DTOs
{
    public class UserDtoWithTokens
    {
        public UserDto User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public UserDtoWithTokens(ApplicationUserModel model, string accessToken, string refreshToken)
        {
            User = new UserDto(model);
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
