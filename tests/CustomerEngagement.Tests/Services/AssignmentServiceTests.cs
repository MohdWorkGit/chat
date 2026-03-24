using CustomerEngagement.Application.Services.Conversations;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Core.Interfaces;
using MediatR;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Tests.Services;

public class AssignmentServiceTests
{
    private readonly Mock<IRepository<Conversation>> _conversationRepoMock;
    private readonly Mock<IRepository<InboxMember>> _inboxMemberRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AssignmentService _sut;

    public AssignmentServiceTests()
    {
        _conversationRepoMock = new Mock<IRepository<Conversation>>();
        _inboxMemberRepoMock = new Mock<IRepository<InboxMember>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mediatorMock = new Mock<IMediator>();
        _sut = new AssignmentService(
            _conversationRepoMock.Object,
            _inboxMemberRepoMock.Object,
            _unitOfWorkMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task AutoAssign_AssignsInRoundRobinRotation()
    {
        var conversation1 = CreateTestConversation(1);
        var conversation2 = CreateTestConversation(2);

        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation2);

        var inboxMembers = new List<InboxMember>
        {
            new() { Id = 1, InboxId = 10, UserId = 100 },
            new() { Id = 2, InboxId = 10, UserId = 200 },
            new() { Id = 3, InboxId = 10, UserId = 300 }
        }.AsReadOnly();

        _inboxMemberRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(inboxMembers);

        await _sut.AutoAssignAsync(1, 10);

        conversation1.AssigneeId.Should().NotBeNull();

        await _sut.AutoAssignAsync(2, 10);

        // The two conversations should be assigned to different agents in rotation
        conversation1.AssigneeId.Should().NotBe(conversation2.AssigneeId);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AssignToAgent_AssignsToSpecificAgent()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.AssignToAgentAsync(1, 42);

        conversation.AssigneeId.Should().Be(42);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignToTeam_AssignsToTeam()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        await _sut.AssignToTeamAsync(1, 7);

        conversation.TeamId.Should().Be(7);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AutoAssign_WhenNoAgentsAvailable_DoesNotAssign()
    {
        var conversation = CreateTestConversation(1);
        _conversationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        _inboxMemberRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InboxMember>().AsReadOnly());

        await _sut.AutoAssignAsync(1, 10);

        conversation.AssigneeId.Should().BeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignToAgent_WhenConversationNotFound_Throws()
    {
        _conversationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var act = () => _sut.AssignToAgentAsync(999, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task AssignToTeam_WhenConversationNotFound_Throws()
    {
        _conversationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        var act = () => _sut.AssignToTeamAsync(999, 1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
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
