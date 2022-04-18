using System.ComponentModel.DataAnnotations;

#nullable disable

namespace CountryhouseService.API.Models
{
    public abstract class Image
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public Uri Source { get; set; }
    }
}
