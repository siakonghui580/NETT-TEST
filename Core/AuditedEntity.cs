namespace Core
{
    public class CreationAuditedEntity
    {
        public DateTime CreationTime { get; set; }
        protected CreationAuditedEntity()
        {
            CreationTime = DateTime.UtcNow;
        }
    }
    public class AuditedEntity : CreationAuditedEntity
    {
        public DateTime? LastModificationTime { get; set; }
    }

    public class FullAuditedEntity : AuditedEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
    }
}
