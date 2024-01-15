namespace UrlShorty.Models
{
    public class ShortenUrlRequestModel
    {
        public string Url { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
    }
}
