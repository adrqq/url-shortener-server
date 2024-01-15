using UrlShorty.Models;

namespace UrlShorty.DTOs
{
    public class UserDto (ApplicationUserModel model)
    {
        public Guid Id { get; set; } = model.Id;
        public string Nickname { get; set; } = model.Nickname;
        public string Email { get; set; } = model.Email;
        public bool IsActivated { get; set; } = model.IsActivated;
        public bool IsAdmin { get; set; } = model.IsAdmin;
    }
}
