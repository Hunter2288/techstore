using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;

namespace Reviews.Domain.ValueObjects;

public sealed record Author : ValueObject
{
    public long UserId { get; }
    public string DisplayName { get; }

    public Author(long userId, string displayName)
    {
        if (userId <= 0)
            throw new DomainException(
                "Ідентифікатор автора має бути більшим за 0.");

        if (string.IsNullOrWhiteSpace(displayName))
            throw new DomainException("Ім'я автора є обов'язковим.");

        displayName = displayName.Trim();

        if (displayName.Length > 100)
            throw new DomainException(
                "Ім'я автора не може перевищувати 100 символів.");

        UserId = userId;
        DisplayName = displayName;
    }
}