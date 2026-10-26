using Reviews.Domain.Common;

namespace Reviews.Domain.ValueObjects;

public sealed record ReviewReply(
    Author Author,
    string Text,
    DateTime CreatedAt) : ValueObject;