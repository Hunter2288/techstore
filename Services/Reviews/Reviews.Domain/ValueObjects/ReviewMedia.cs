using Reviews.Domain.Common;

namespace Reviews.Domain.ValueObjects;

public sealed record ReviewMedia(string Type, string Url)
    : ValueObject;