using System.ComponentModel.DataAnnotations;

namespace Lepilina.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Логин обязателен")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Логин должен содержать только английские буквы и цифры.")]
        public string Login { get; set; }

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[0-9]).+$", ErrorMessage = "Пароль должен содержать хотя бы одну цифру.")]
        public string Password { get; set; }
        [Required]
        public string Sername { get; set; }

        [Required]
        public string Names { get; set; } 

        public string Patronymic { get; set; } 
    }
}

