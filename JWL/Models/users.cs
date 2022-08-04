using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class users
    {
        [Key]
        public int userId { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Name")]
        public string name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter Email")]
        [EmailAddress(ErrorMessage = "Please enter valid Email")]
        public string email { get; set; }
        [Required(ErrorMessage = "Please enter Password")]
        [RegularExpression(pattern: @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$", ErrorMessage = "Please enter Strong Password")]
        public string password { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Please enter Confirm Password")]
        [Compare("password", ErrorMessage = "Password and Confirm Password must be same")]
        public string confirmPassword { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please select User Type")]
        public UserType userType { get; set; }
        public bool? isFirstTime { get; set; }
        public bool? isDeleted { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please Select Active Users")]
        public bool isActive { get; set; }
    }
    public enum UserType
    {
        [Display(Name ="Admin")]
        Admin = 1,
        [Display(Name = "Carrier User")]
        CarrierUser
    }
}
