using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models
{
    public class errortracelog
    {
        [Key]
        public int errorId { get; set; }
        public string errorName { get; set; }
        public string errorControl { get; set; }
        public string errorMessage { get; set; }
        public string errorStack { get; set; }
        public DateTime errorTime { get; set; }
        public string error { get; set; }


    }
}
