using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class vehicltype
    {
        [Key]
        public int vehicleId { get; set; }
        [Required(ErrorMessage = "Please enter Vehicle Types", AllowEmptyStrings = false)]
        public string vehicleName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Vehicle Types", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
    }
}
