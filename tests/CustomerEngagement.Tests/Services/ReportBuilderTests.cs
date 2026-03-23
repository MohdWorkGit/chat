using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Reporting;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Tests.Services;

public class ReportBuilderTests
{
    private readonly Mock<IRepository<ReportingEvent>> _reportingEventRepoMock;
    private readonly Mock<IRepository<Conversation>> _conversationRepoMock;
    private readonly Mock<IRepository<Message>> _messageRepoMock;
    private readonly Mock<ILogger<ReportBuilder>> _loggerMock;
    private readonly ReportBuilder _sut;

    public ReportBuilderTests()
    {
        _reportingEventRepoMock = new Mock<IRepository<ReportingEvent>>();
        _conversationRepoMock = new Mock<IRepository<Conversation>>();
        _messageRepoMock = new Mock<IRepository<Message>>();
        _loggerMock = new Mock<ILogger<ReportBuilder>>();
        _sut = new ReportBuilder(
            _reportingEventRepoMock.Object,
            _conversationRepoMock.Object,
            _messageRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetConversationReport_ReturnsCorrectCounts()
    {
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var filter = new ReportFilterDto { Since = since, Until = until, GroupBy = "day" };

        var events = new List<ReportingEvent>
        {
            new() { Id = 1, AccountId = 1, CreatedAt = new DateTime(2024, 1, 1, 10, 0, 0) },
            new() { Id = 2, AccountId = 1, CreatedAt = new DateTime(2024, 1, 1, 14, 0, 0) },
            new() { Id = 3, AccountId = 1, CreatedAt = new DateTime(2024, 1, 2, 9, 0, 0) }
        }.AsReadOnly();

        _reportingEventRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _sut.GetConversationReportAsync(1, filter);

        result.ReportName.Should().Be("Conversations");
        result.Total.Should().Be(3);
        result.DataPoints.Should().HaveCountGreaterOrEqualTo(2);
        // First day should have 2 events
        result.DataPoints[0].Value.Should().Be(2);
        // Second day should have 1 event
        result.DataPoints[1].Value.Should().Be(1);
    }

    [Fact]
    public async Task GetAgentReport_ReturnsPerAgentData()
    {
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var filter = new ReportFilterDto { Since = since, Until = until, GroupBy = "day", AgentId = 5 };

        var events = new List<ReportingEvent>
        {
            new() { Id = 1, AccountId = 1, UserId = 5, CreatedAt = new DateTime(2024, 1, 1, 8, 0, 0) },
            new() { Id = 2, AccountId = 1, UserId = 5, CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0) }
        }.AsReadOnly();

        _reportingEventRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _sut.GetAgentReportAsync(1, filter);

        result.ReportName.Should().Be("Agent Activity");
        result.Total.Should().Be(2);
        result.DataPoints.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSummary_ReturnsAggregatedData()
    {
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc);
        var filter = new ReportFilterDto { Since = since, Until = until, GroupBy = "day" };

        var conversations = new List<Conversation>
        {
            new() { Id = 1, AccountId = 1, InboxId = 1, ContactId = 1, Status = ConversationStatus.Open, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, AccountId = 1, InboxId = 1, ContactId = 2, Status = ConversationStatus.Resolved, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, AccountId = 1, InboxId = 1, ContactId = 3, Status = ConversationStatus.Resolved, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, AccountId = 1, InboxId = 1, ContactId = 4, Status = ConversationStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsReadOnly();

        var messages = new List<Message>
        {
            new() { Id = 1, ConversationId = 1, AccountId = 1, MessageType = MessageType.Incoming, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, ConversationId = 1, AccountId = 1, MessageType = MessageType.Outgoing, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, ConversationId = 2, AccountId = 1, MessageType = MessageType.Incoming, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, ConversationId = 2, AccountId = 1, MessageType = MessageType.Outgoing, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, ConversationId = 3, AccountId = 1, MessageType = MessageType.Outgoing, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        }.AsReadOnly();

        _conversationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversations);
        _messageRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        var result = await _sut.GetSummaryAsync(1, filter);

        result.TotalConversations.Should().Be(4);
        result.ResolvedConversations.Should().Be(2);
        result.OpenConversations.Should().Be(1);
        result.PendingConversations.Should().Be(1);
        result.TotalMessages.Should().Be(5);
        result.TotalIncomingMessages.Should().Be(2);
        result.TotalOutgoingMessages.Should().Be(3);
    }

    [Fact]
    public async Task GetConversationReport_DateRangeFiltering_ReturnsCorrectGrouping()
    {
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = new DateTime(2024, 1, 14, 0, 0, 0, DateTimeKind.Utc);
        var filter = new ReportFilterDto { Since = since, Until = until, GroupBy = "week" };

        var events = new List<ReportingEvent>
        {
            new() { Id = 1, AccountId = 1, CreatedAt = new DateTime(2024, 1, 2, 10, 0, 0) },
            new() { Id = 2, AccountId = 1, CreatedAt = new DateTime(2024, 1, 3, 10, 0, 0) },
            new() { Id = 3, AccountId = 1, CreatedAt = new DateTime(2024, 1, 9, 10, 0, 0) },
            new() { Id = 4, AccountId = 1, CreatedAt = new DateTime(2024, 1, 10, 10, 0, 0) },
            new() { Id = 5, AccountId = 1, CreatedAt = new DateTime(2024, 1, 11, 10, 0, 0) }
        }.AsReadOnly();

        _reportingEventRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(events);

        var result = await _sut.GetConversationReportAsync(1, filter);

        result.Total.Should().Be(5);
        result.GroupBy.Should().Be("week");
        result.Since.Should().Be(since);
        result.Until.Should().Be(until);
        // Week grouping should produce data points
        result.DataPoints.Should().NotBeEmpty();
        // First week (Jan 1-7) should have 2 events
        result.DataPoints[0].Value.Should().Be(2);
        // Second week (Jan 8-14) should have 3 events
        result.DataPoints[1].Value.Should().Be(3);
    }

    [Fact]
    public async Task GetConversationReport_EmptyDateRange_ReturnsZeroCounts()
    {
        var since = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var until = new DateTime(2024, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var filter = new ReportFilterDto { Since = since, Until = until, GroupBy = "day" };

        _reportingEventRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ReportingEvent>().AsReadOnly());

        var result = await _sut.GetConversationReportAsync(1, filter);

        result.Total.Should().Be(0);
        result.DataPoints.Should().AllSatisfy(dp => dp.Value.Should().Be(0));
    }
}
