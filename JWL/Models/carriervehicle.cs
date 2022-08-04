using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class carriervehicle
    {
        [Key]
        public int carrierVehicleId { get; set; }
        public int carrierId { get; set; }
        public int vehicleId { get; set; }
        public int numberOfVehicle { get; set; }
        public bool? isDeleted { get; set; }
        [ForeignKey("vehicleId")]
        public vehicltype vehicltype { get; set; }
        [ForeignKey("carrierId")]
        public carrierusers carrierusers { get; set; }
    }
}
