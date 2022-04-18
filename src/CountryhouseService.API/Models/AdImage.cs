using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class AdImage : Image
    {
        [ForeignKey("Ad")]
        public int? AdId { get; set; }

        public Ad Ad { get; set; }

        // Order 1 is reserved for preview image
        [Range(2, 8, ErrorMessage = "Cannot load more than {2} images")]
        public byte Order { get; set; }
    }
}