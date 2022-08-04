using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class cargospecialties
    {
        [Key]
        public int cargoId { get; set; }
        [Required(ErrorMessage = "Please enter Cargo Specialities", AllowEmptyStrings = false)]
        public string cargoName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Cargo Specialities", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
    }
}
