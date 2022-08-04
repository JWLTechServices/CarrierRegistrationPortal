using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class carrierTrailer
    {
        public int carrierId { get; set; }
        public int trailerId { get; set; }
        public int numberOfVehicle { get; set; }
        [Key]
        public int carrierTrailerId { get; set; }
        public bool? isDeleted { get; set; }
        [ForeignKey("trailerId")]
        public trailer trailer { get; set; }
        [ForeignKey("carrierId")]
        public carrierusers carrierusers { get; set; }
    }
}
