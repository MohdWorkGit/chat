namespace CustomerEngagement.Application.DTOs;

public class PortalDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string? HeaderText { get; set; }
    public string? PageTitle { get; set; }
    public string? HomepageLink { get; set; }
    public string? Color { get; set; }
    public string? LogoUrl { get; set; }
    public string? LogoContentType { get; set; }
    public bool IsArchived { get; set; }
    public int ArticleCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePortalRequest
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string? HeaderText { get; set; }
    public string? PageTitle { get; set; }
    public string? HomepageLink { get; set; }
    public string? Color { get; set; }
}

public class UpdatePortalRequest
{
    public string? Name { get; set; }
    public string? CustomDomain { get; set; }
    public string? HeaderText { get; set; }
    public string? PageTitle { get; set; }
    public string? HomepageLink { get; set; }
    public string? Color { get; set; }
    public bool? IsArchived { get; set; }
}
