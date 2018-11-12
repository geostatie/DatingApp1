using System.ComponentModel.DataAnnotations;

namespace DatingApp1.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(8 , MinimumLength = 4, ErrorMessage = "You must specify password between ")]
        public string Password { get; set; }
    }
}