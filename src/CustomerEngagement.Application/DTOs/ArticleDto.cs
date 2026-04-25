namespace CustomerEngagement.Application.DTOs;

public class ArticleDto
{
    public int Id { get; set; }
    public int PortalId { get; set; }
    public int AccountId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public int? CategoryId { get; set; }
    public int? AuthorId { get; set; }
}

public class UpdateArticleRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? CategoryId { get; set; }
}
