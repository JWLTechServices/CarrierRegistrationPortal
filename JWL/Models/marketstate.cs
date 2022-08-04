using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class marketstate
    {
        [Key]
        public int marketStateID { get; set; }
        [Required(ErrorMessage = "Please enter Operating States", AllowEmptyStrings = false)]
        public string marketName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Operating States", AllowEmptyStrings = false)]
        public bool isActive { get; set; }

    }
}
