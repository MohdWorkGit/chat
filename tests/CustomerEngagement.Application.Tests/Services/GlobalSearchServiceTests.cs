using System.Linq.Expressions;
using CustomerEngagement.Application.Services.Search;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerEngagement.Application.Tests.Services;

public class GlobalSearchServiceTests
{
    private readonly Mock<IRepository<Conversation>> _conversationRepoMock;
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<Message>> _messageRepoMock;
    private readonly Mock<ILogger<GlobalSearchService>> _loggerMock;
    private readonly GlobalSearchService _sut;

    public GlobalSearchServiceTests()
    {
        _conversationRepoMock = new Mock<IRepository<Conversation>>();
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _messageRepoMock = new Mock<IRepository<Message>>();
        _loggerMock = new Mock<ILogger<GlobalSearchService>>();
        _sut = new GlobalSearchService(
            _conversationRepoMock.Object,
            _contactRepoMock.Object,
            _messageRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SearchAsync_WhenQueryIsEmpty_ReturnsEmptyResult()
    {
        var result = await _sut.SearchAsync(1, "");

        result.TotalCount.Should().Be(0);
        result.Conversations.Should().BeEmpty();
        result.Contacts.Should().BeEmpty();
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_WhenQueryIsWhitespace_ReturnsEmptyResult()
    {
        var result = await _sut.SearchAsync(1, "   ");

        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ReturnsResultsFromAllRepositories()
    {
        _conversationRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>
            {
                new() { Id = 1, AccountId = 10, InboxId = 1, ContactId = 1, Identifier = "test-conv", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact>
            {
                new() { Id = 1, AccountId = 10, Name = "Test User", Email = "test@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });

        _messageRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Message, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>
            {
                new() { Id = 1, AccountId = 10, ConversationId = 1, Content = "test message", CreatedAt = DateTime.UtcNow }
            });

        var result = await _sut.SearchAsync(10, "test");

        result.TotalCount.Should().Be(3);
        result.Conversations.Should().HaveCount(1);
        result.Contacts.Should().HaveCount(1);
        result.Messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchContactsAsync_ReturnsOnlyContacts()
    {
        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact>
            {
                new() { Id = 1, AccountId = 10, Name = "Alice", Email = "alice@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new() { Id = 2, AccountId = 10, Name = "Bob", Email = "bob@example.com", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });

        var result = await _sut.SearchContactsAsync(10, "example");

        result.TotalCount.Should().Be(2);
        result.Contacts.Should().HaveCount(2);
        result.Conversations.Should().BeEmpty();
        result.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_RespectsPageSize()
    {
        var contacts = Enumerable.Range(1, 50).Select(i => new Contact
        {
            Id = i,
            AccountId = 10,
            Name = $"Contact {i}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        _contactRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contacts);
        _conversationRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Conversation>());
        _messageRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Message, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        var result = await _sut.SearchAsync(10, "contact", page: 1, pageSize: 10);

        result.Contacts.Should().HaveCount(10);
        result.TotalCount.Should().Be(50);
    }
}
