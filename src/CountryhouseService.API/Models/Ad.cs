using CountryhouseService.API.Defaults;
using CountryhouseService.API.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable
namespace CountryhouseService.API.Models
{
    public class Ad
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        [Required]
        public string Title { get; set; }

        [MaxLength(2000, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public string Description { get; set; }

        [MaxLength(100, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        [Required]
        public string Address { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = ErrorMessagesTemplates.NUMBER_NEGATIVE)]
        public int Budget { get; set; }

        [MaxLength(25, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        [Required]
        [DataType(DataType.PhoneNumber)]
        public string ContactNumber { get; set; }

        public ICollection<AdImage> NonPreviewImages { get; set; }

        public Uri PreviewImageSource { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        [Required]
        public DateTime UpdatedOn { get; set; }

        [ForeignKey("Author")]
        [Required]
        public string AuthorId { get; set; }

        public User Author { get; set; }

        [ForeignKey("Status")]
        [Required]
        public int StatusId { get; set; }

        public AdStatus Status { get; set; }

        [LaterThanTodayOrToday]
        [Column(TypeName = "date")]
        public DateTime? AccomplishFromDate { get; set; }

        [LaterThanTodayOrToday]
        [Column(TypeName = "date")]
        public DateTime? AccomplishUntilDate { get; set; }

        public ICollection<Request> Requests { get; set; }
    }
}
