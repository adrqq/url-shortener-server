namespace UrlShorty.Models
{
    public class RegisterRequestModel
    {
        public string Nickname { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string AdminCredentials { get; set; } = String.Empty;
    }
}
