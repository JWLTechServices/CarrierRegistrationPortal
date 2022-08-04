using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Text;

namespace Models
{
    public class role
    {
        [Key]
        public int roleId { get; set; }
        public string name { get; set; }
        public string details { get; set; }
    }
}
