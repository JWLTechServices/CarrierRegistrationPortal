using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class trailer
    {
        [Key]
        public int trailerId { get; set; }
        [Required(ErrorMessage = "Please enter Trailer Types", AllowEmptyStrings = false)]
        public string trailerName { get; set; }
        public bool? isDeleted { get; set; }
        [Required(ErrorMessage = "Please Select Active Trailer Types", AllowEmptyStrings = false)]
        public bool isActive { get; set; }
    }
}
