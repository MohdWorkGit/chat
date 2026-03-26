using System.Text;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class ContactImportServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ContactImportService>> _loggerMock;
    private readonly ContactImportService _sut;

    public ContactImportServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ContactImportService>>();
        _sut = new ContactImportService(
            _contactRepoMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithValidCsv_ImportsContacts()
    {
        var csv = "name,email,phone,company,location\nJohn Doe,john@example.com,+1234567890,Acme,NYC\nJane Doe,jane@example.com,+0987654321,Globex,LA";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.TotalRows.Should().Be(2);
        result.ImportedCount.Should().Be(2);
        result.SkippedCount.Should().Be(0);
        result.ErrorCount.Should().Be(0);
        result.Errors.Should().BeEmpty();
        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithEmptyFile_ReturnsErrorResult()
    {
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(""));

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.TotalRows.Should().Be(0);
        result.ImportedCount.Should().Be(0);
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("empty");
    }

    [Fact]
    public async Task ImportFromCsvAsync_RowWithoutEmailOrPhone_IsSkipped()
    {
        var csv = "name,email,phone,company,location\nJohn Doe,,,,NYC";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.TotalRows.Should().Be(1);
        result.ImportedCount.Should().Be(0);
        result.SkippedCount.Should().Be(1);
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("no email or phone");
    }

    [Fact]
    public async Task ImportFromCsvAsync_WithDuplicateEmails_ImportsAll()
    {
        // The service does not deduplicate; it imports all rows
        var csv = "name,email,phone\nJohn,john@example.com,+111\nJohn2,john@example.com,+222";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.ImportedCount.Should().Be(2);
        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ImportFromCsvAsync_EmptyRow_IsSkipped()
    {
        var csv = "name,email,phone\nJohn,john@example.com,+111\n\nJane,jane@example.com,+222";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => c);

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.TotalRows.Should().Be(3);
        result.ImportedCount.Should().Be(2);
        result.SkippedCount.Should().Be(1);
    }

    [Fact]
    public async Task ImportFromCsvAsync_WhenAddThrows_RecordsError()
    {
        var csv = "name,email,phone\nJohn,john@example.com,+111";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        _contactRepoMock.Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.ImportFromCsvAsync(stream, 10);

        result.TotalRows.Should().Be(1);
        result.ImportedCount.Should().Be(0);
        result.ErrorCount.Should().Be(1);
        result.Errors.Should().ContainSingle()
            .Which.Should().Contain("DB error");
    }
}
