using CountryhouseService.API.Defaults;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class Request
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(2000, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public string Comment { get; set; }

        [ForeignKey("Contractor")]
        [Required]
        public string ContractorId { get; set; }
        
        public User Contractor { get; set; }

        [ForeignKey("Ad")]
        public int? AdId { get; set; }

        public Ad Ad { get; set; }

        [ForeignKey("Status")]
        [Required]
        public int StatusId { get; set; }

        public RequestStatus Status { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime UpdatedOn { get; set; }
    }
}
