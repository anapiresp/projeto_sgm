using System.ComponentModel.DataAnnotations;

namespace SeaRise.Models.DataTransferObjects
{
    public class TestimonyCommentCreateDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(1000, ErrorMessage = "Content can't be longer than 1000 characters.")]
        public string Content { get; set; } = string.Empty;
    }
}
