using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class state
    {
        public int stateId { get; set; }
        [Required(ErrorMessage = "Please enter States", AllowEmptyStrings = false)]
        public string stateName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Active Select States", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
    }
}
