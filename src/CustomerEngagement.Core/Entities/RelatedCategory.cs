namespace CustomerEngagement.Core.Entities;

public class RelatedCategory : BaseEntity
{
    public int CategoryId { get; set; }
    public int RelatedCategoryId { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Category Related { get; set; } = null!;
}
