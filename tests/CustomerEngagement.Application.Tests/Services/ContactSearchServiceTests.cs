using System.Linq.Expressions;
using CustomerEngagement.Application.Services.Contacts;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class ContactSearchServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<ILogger<ContactSearchService>> _loggerMock;
    private readonly ContactSearchService _sut;

    public ContactSearchServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _loggerMock = new Mock<ILogger<ContactSearchService>>();
        _sut = new ContactSearchService(
            _contactRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatchingContacts()
    {
        var contacts = new List<Contact>
        {
            CreateTestContact(1, "Alice Smith", "alice@example.com"),
            CreateTestContact(2, "Bob Jones", "bob@example.com"),
            CreateTestContact(3, "Alice Wonder", "wonder@example.com")
        };

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Contact, bool>> predicate, CancellationToken _) =>
                contacts.Where(predicate.Compile()).ToList().AsReadOnly());

        var result = await _sut.SearchAsync(10, "alice");

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task SearchAsync_ByEmail_ReturnsMatchingContacts()
    {
        var contacts = new List<Contact>
        {
            CreateTestContact(1, "Alice", "alice@example.com"),
            CreateTestContact(2, "Bob", "bob@test.com")
        };

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<Contact, bool>> predicate, CancellationToken _) =>
                contacts.Where(predicate.Compile()).ToList().AsReadOnly());

        var result = await _sut.SearchAsync(10, "bob@test");

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].Email.Should().Be("bob@test.com");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsEmpty()
    {
        var result = await _sut.SearchAsync(10, "");

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        _contactRepoMock.Verify(
            r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhitespaceQuery_ReturnsEmpty()
    {
        var result = await _sut.SearchAsync(10, "   ");

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        var contacts = Enumerable.Range(1, 10)
            .Select(i => CreateTestContact(i, $"Contact {i}", $"contact{i}@example.com"))
            .ToList();

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts.AsReadOnly());

        var result = await _sut.SearchAsync(10, "contact", page: 2, pageSize: 3);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(10);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task SearchAsync_LastPagePartial_ReturnsRemainingItems()
    {
        var contacts = Enumerable.Range(1, 5)
            .Select(i => CreateTestContact(i, $"Contact {i}", $"contact{i}@example.com"))
            .ToList();

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts.AsReadOnly());

        var result = await _sut.SearchAsync(10, "contact", page: 2, pageSize: 3);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetRecentAsync_ReturnsRecentContacts()
    {
        var contacts = Enumerable.Range(1, 5)
            .Select(i => CreateTestContact(i, $"Contact {i}", $"contact{i}@example.com"))
            .ToList();

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts.AsReadOnly());

        var result = await _sut.GetRecentAsync(10, 3);

        result.Should().HaveCount(3);
    }

    private static Contact CreateTestContact(int id, string name, string email) => new()
    {
        Id = id,
        AccountId = 10,
        Name = name,
        Email = email,
        Phone = $"+100000000{id}",
        ContactType = ContactType.Visitor,
        CreatedAt = DateTime.UtcNow.AddMinutes(-id),
        UpdatedAt = DateTime.UtcNow.AddMinutes(-id)
    };
}
