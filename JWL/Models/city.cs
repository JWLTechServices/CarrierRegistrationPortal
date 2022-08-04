using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class city
    {
        public int cityId { get; set; }
        [Required(ErrorMessage = "Please enter Cities", AllowEmptyStrings = false)]
        public string cityName { get; set; }
        [Required(ErrorMessage = "Please select States", AllowEmptyStrings = false)]
        public int stateId { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Cities", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
        [ForeignKey("stateId")]
        public state State { get; set; }
    }
}
