using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace SeaRise.Models.DataTransferObjects
{
    public class TestimonyCreateDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, ErrorMessage = "Content can't be longer than 2000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Url]
        public string? MediaUrl { get; set; }
    }
}

