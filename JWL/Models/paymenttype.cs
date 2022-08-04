using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class paymenttype
    {
        [Key]
        public int paymentTypeId { get; set; }
        [Required(ErrorMessage = "Please enter Payment Methods", AllowEmptyStrings = false)]
        public string paymentName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Payment Methods", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
    }
}
