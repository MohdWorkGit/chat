using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Moq;
using FluentAssertions;

namespace CustomerEngagement.Application.Tests.Services;

public class ConversationServiceTests
{
    private readonly Mock<IRepository<Conversation>> _conversationRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly ConversationService _sut;

    public ConversationServiceTests()
    {
        _conversationRepoMock = new Mock<IRepository<Conversation>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new ConversationService(
            _conversationRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenConversationExists_ReturnsDto()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.AccountId.Should().Be(conversation.AccountId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenConversationNotFound_ReturnsNull()
    {
        _conversationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_CreatesConversationAndPublishesEvent()
    {
        var request = new CreateConversationRequest
        {
            InboxId = 1,
            ContactId = 2,
            AssigneeId = 3,
            TeamId = 4
        };

        _conversationRepoMock.Setup(r => r.AddAsync(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation c, CancellationToken _) => c);

        var result = await _sut.CreateAsync(10, request);

        result.Should().NotBeNull();
        result.InboxId.Should().Be(1);
        result.ContactId.Should().Be(2);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ConversationCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_UpdatesAssignmentAndPublishesEvent()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.AssignAsync(1, 5, 10);

        conversation.AssigneeId.Should().Be(5);
        conversation.TeamId.Should().Be(10);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<ConversationAssignedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_WhenConversationNotFound_Throws()
    {
        _conversationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var act = () => _sut.AssignAsync(999, 1, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task MuteAsync_SetsMutedToTrue()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.MuteAsync(1);

        conversation.Muted.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TogglePriorityAsync_TogglesFromNoneToUrgent()
    {
        var conversation = CreateTestConversation(1);
        conversation.Priority = ConversationPriority.None;
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.TogglePriorityAsync(1);

        conversation.Priority.Should().Be(ConversationPriority.Urgent);
    }

    [Fact]
    public async Task SnoozeAsync_SetsStatusToSnoozed()
    {
        var conversation = CreateTestConversation(1);
        var snoozeUntil = DateTime.UtcNow.AddHours(2);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.SnoozeAsync(1, snoozeUntil);

        conversation.Status.Should().Be(ConversationStatus.Snoozed);
        conversation.SnoozedUntil.Should().Be(snoozeUntil);
    }

    private static Conversation CreateTestConversation(int id) => new()
    {
        Id = id,
        AccountId = 10,
        InboxId = 1,
        ContactId = 2,
        Status = ConversationStatus.Open,
        Priority = ConversationPriority.None,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
