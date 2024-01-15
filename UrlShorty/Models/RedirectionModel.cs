using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace UrlShorty.Models
{
    public class RedirectionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Url { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
