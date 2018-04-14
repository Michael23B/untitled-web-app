using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Models.ViewModels.Account
{
    public class LoginUserViewModel
    {
        [Required]
        [DisplayName("User name")]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }
    }
}