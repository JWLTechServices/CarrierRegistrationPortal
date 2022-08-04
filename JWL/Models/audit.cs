using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class audit
    {
        [Key]
        [Column("auditId")]
        public int auditId { get; set; }
        [Column("auditDateTime")]
        public DateTime auditDateTime { get; set; }
        [Column("auditType")]
        public string auditType { get; set; }
        [Column("auditUser")]
        public int? auditUser { get; set; }
        [Column("tableName")]
        public string tableName { get; set; }
        [Column("keyValues")]
        public string keyValues { get; set; }
        [Column("oldValues")]
        public string oldValues { get; set; }
        [Column("newValues")]
        public string newValues { get; set; }
        [Column("changedColumns")]
        public string changedColumns { get; set; }
        [Column("pageName")]
        public string pageName { get; set; }
        [ForeignKey("auditUser")]
        public users Users { get; set; }
        [NotMapped]
        public string notes { get; set; }
    }
    public enum AuditType
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3
    }
}
