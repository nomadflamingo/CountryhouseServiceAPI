using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class AdStatus : Status
    {
        public ICollection<Ad> Ads { get; set; }
    }
}
