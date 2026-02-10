namespace Verendar.Common.Databases.Interfaces
{
    public interface IEntity
    {
        public Guid Id { get; set; }
    }

    public interface IAuditableEntity : IEntity
    {
        DateTime CreatedAt { get; set; }
        Guid CreatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
        Guid? UpdatedBy { get; set; }
    }

    public interface ISoftDeleteEntity : IEntity
    {
        DateTime? DeletedAt { get; set; }
        Guid? DeletedBy { get; set; }
    }
}
