using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace CountryhouseService.API.Models
{
    public class RequestStatus : Status
    {
        public ICollection<Request> Requests { get; set; }
    }
}