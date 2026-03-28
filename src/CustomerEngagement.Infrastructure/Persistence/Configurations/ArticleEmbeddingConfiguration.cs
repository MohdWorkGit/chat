using CustomerEngagement.Enterprise.Captain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CustomerEngagement.Infrastructure.Persistence.Configurations;

public class ArticleEmbeddingConfiguration : IEntityTypeConfiguration<ArticleEmbedding>
{
    public void Configure(EntityTypeBuilder<ArticleEmbedding> builder)
    {
        builder.ToTable("article_embeddings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Embedding)
            .HasColumnType("vector(1536)")
            .IsRequired();

        builder.Property(e => e.ChunkText)
            .IsRequired()
            .HasMaxLength(8000);

        builder.HasIndex(e => e.ArticleId);
    }
}
