using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{

    public class authorizedpath
    {
        [Key]
        public int authorizedId { get; set; }
        public int carrierId { get; set; }
        public string documentPath { get; set; }
        public string selectedOptions { get; set; }
        public bool? isDeleted { get; set; }
        [ForeignKey("carrierId")]
        public carrierusers carrierusers { get; set; }
    }
}
