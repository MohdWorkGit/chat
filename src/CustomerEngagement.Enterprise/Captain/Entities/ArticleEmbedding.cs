using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CustomerEngagement.Core.Entities;
using Pgvector;

namespace CustomerEngagement.Enterprise.Captain.Entities;

public class ArticleEmbedding : BaseEntity
{
    public int ArticleId { get; set; }

    [Column(TypeName = "vector(1536)")]
    public Vector Embedding { get; set; } = null!;

    [Required]
    [MaxLength(8000)]
    public required string ChunkText { get; set; }

    // Navigation property - Article entity from help center module
    // public Article Article { get; set; } = null!;
}
