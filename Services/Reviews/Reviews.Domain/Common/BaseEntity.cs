using Reviews.Domain.Exceptions;

namespace Reviews.Domain.Common;

public abstract class BaseEntity
{
    public string Id { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected BaseEntity()
        : this(Guid.NewGuid().ToString("N")[..24], DateTime.UtcNow)
    {
    }

    protected BaseEntity(string id, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("Ідентифікатор не може бути порожнім.");

        Id = id;
        CreatedAt = createdAt.Kind == DateTimeKind.Utc
            ? createdAt
            : createdAt.ToUniversalTime();
    }
}
