using System.ComponentModel.DataAnnotations;
using Verender.Common.Databases.Interfaces;

namespace Verender.Common.Databases.Base
{
    public abstract class BaseEntity : IAuditableEntity, ISoftDeleteEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
    }

    public enum EntityStatus
    {
        Active = 1,
        Inactive = 2,
        Deleted = 3
    }
}