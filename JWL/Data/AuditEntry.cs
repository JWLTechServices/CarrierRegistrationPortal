using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data
{
    public class AuditEntry
    {
        public EntityEntry Entry { get; }
        public AuditType AuditType { get; set; }
        public string AuditUser { get; set; }
        public string TableName { get; set; }
        public Dictionary<string, object>
               KeyValues
        { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>
               OldValues
        { get; } = new Dictionary<string, object>();
        public Dictionary<string, object>
               NewValues
        { get; } = new Dictionary<string, object>();
        public List<string> ChangedColumns { get; } = new List<string>();

        public AuditEntry(EntityEntry entry, string auditUser)
        {
            Entry = entry;
            AuditUser = auditUser;
            SetChanges();
        }

        private void SetChanges()
        {
            TableName = Entry.Metadata.GetTableName();
            //TableName = Entry.Metadata.Relational().TableName;
            foreach (PropertyEntry property in Entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                string dbColumnName = property.Metadata.GetColumnName();
                //string dbColumnName = property.Metadata.Relational().ColumnName;

                if (property.Metadata.IsPrimaryKey())
                {
                    KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (Entry.State)
                {
                    case EntityState.Added:
                        NewValues[propertyName] = property.CurrentValue;
                        AuditType = AuditType.Create;
                        break;

                    case EntityState.Deleted:
                        OldValues[propertyName] = property.OriginalValue;
                        AuditType = AuditType.Delete;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            ChangedColumns.Add(dbColumnName);

                            OldValues[propertyName] = property.OriginalValue;
                            NewValues[propertyName] = property.CurrentValue;
                            AuditType = AuditType.Update;
                        }
                        break;
                }
            }
        }

        public audit ToAudit()
        {
            if (ChangedColumns != null && ChangedColumns.Count > 0 && ChangedColumns[0] == "isDeleted")
            {
                AuditType = AuditType.Delete;
            }
            var audit = new audit();
            audit.auditDateTime = DateTime.Now;
            audit.auditType = AuditType.ToString();
            audit.auditUser = Convert.ToInt32(AuditUser.ToString());
            audit.tableName = TableName.ToString();
            audit.keyValues = JsonConvert.SerializeObject(KeyValues).ToString();
            audit.oldValues = OldValues.Count == 0 ?
                              string.Empty : JsonConvert.SerializeObject(OldValues).ToString();
            audit.newValues = NewValues.Count == 0 ?
                              string.Empty : JsonConvert.SerializeObject(NewValues).ToString();
            audit.changedColumns = ChangedColumns.Count == 0 ?
                                   string.Empty : JsonConvert.SerializeObject(ChangedColumns).ToString();

            return audit;
        }
    }
}
