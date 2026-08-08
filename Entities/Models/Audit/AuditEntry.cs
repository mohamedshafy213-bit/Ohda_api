using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Entities.Models.Audit
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
            Action = entry.State.ToString();
            Changes = new List<string>();
            TABLE_ID = new List<string>();
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public string Action { get; }
        public List<string> Changes { get; }
        public List<string> TABLE_ID { get; }
    }
}
