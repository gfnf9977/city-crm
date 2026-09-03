using System.ComponentModel.DataAnnotations;

namespace CityCrm.AdminUI.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Введіть логін")]
        public string Username { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Введіть пароль")]
        [MinLength(6, ErrorMessage = "Мінімум 6 символів")]
        public string Password { get; set; } = string.Empty;
        
        public string Role { get; set; } = "Admin";
        public bool IsActive { get; set; } = true;
    }
}