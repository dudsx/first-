using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        [Compare(nameof(Password), ErrorMessage = "2次密码输入不一致")]
        public string ConfirmPassword { get; set; }
    }
}
