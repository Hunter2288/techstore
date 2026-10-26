using Reviews.Domain.Common;
using Reviews.Domain.Exceptions;
using Reviews.Domain.ValueObjects;

namespace Reviews.Domain.Entities;

public sealed class Comment : BaseEntity
{
    public string DiscussionId { get; private set; } = string.Empty;
    public string? ParentId { get; private set; }
    public Author Author { get; private set; } = null!;
    public string Text { get; private set; } = string.Empty;

    private Comment()
    {
    }

    public Comment(string discussionId, string? parentId, Author author, string text)
    {
        if (string.IsNullOrWhiteSpace(discussionId))
            throw new DomainException("DiscussionId є обов'язковим.");
        if (author is null)
            throw new DomainException("Автор коментаря є обов'язковим.");
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Текст коментаря є обов'язковим.");

        DiscussionId = discussionId;
        ParentId = parentId;
        Author = author;
        Text = text.Trim();
    }
}
