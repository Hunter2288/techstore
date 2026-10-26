namespace Reviews.Application.Common.Exceptions;

public sealed class ReviewNotFoundException : Exception
{
    public ReviewNotFoundException(string message) : base(message)
    {
    }
}
