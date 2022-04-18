using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class Avatar : Image
    {
        [ForeignKey("User")]
        public string UserId { get; set; }

        public User User { get; set; }
    }
}
