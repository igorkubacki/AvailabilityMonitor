using System.ComponentModel.DataAnnotations;

namespace AvailabilityMonitor.Models
{
    public class LoginModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
