using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable
namespace CountryhouseService.API.Models
{
    public abstract class Status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Name { get; set; }
    }
}
