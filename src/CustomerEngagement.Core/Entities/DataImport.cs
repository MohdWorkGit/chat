using System.ComponentModel.DataAnnotations;

namespace CustomerEngagement.Core.Entities;

public class DataImport : BaseEntity
{
    public int AccountId { get; set; }

    [MaxLength(100)]
    public string? DataType { get; set; }

    public int Status { get; set; }
    public int ProcessedRecords { get; set; }
    public int TotalRecords { get; set; }

    // Navigation properties
    public Account Account { get; set; } = null!;
}
