using CountryhouseService.API.Defaults;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class User : IdentityUser
    {
        [Required]
        [EmailAddress]
        public override string Email { get => base.Email; set => base.Email = value; }

        [Required]
        [DataType(DataType.Password)]
        public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }

        [Required]
        [Phone]
        [Column(TypeName = "varchar(30)")]
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }

        [Required]
        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public string LastName { get; set; }

        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public Uri PreviewAvatarSource { get; set; }

        [MaxLength(256, ErrorMessage = ErrorMessagesTemplates.STRING_TOO_LONG)]
        public ICollection<Avatar> Avatars { get; set; }
    }
}
