using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Domain;

public class ConversationEntityTests
{
    [Fact]
    public void Resolve_FromOpen_SetsStatusToResolved()
    {
        var conversation = CreateConversation(ConversationStatus.Open);

        conversation.Resolve();

        conversation.Status.Should().Be(ConversationStatus.Resolved);
        conversation.SnoozedUntil.Should().BeNull();
    }

    [Fact]
    public void Resolve_FromResolved_ThrowsInvalidOperation()
    {
        var conversation = CreateConversation(ConversationStatus.Resolved);

        var act = () => conversation.Resolve();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reopen_FromResolved_SetsStatusToOpen()
    {
        var conversation = CreateConversation(ConversationStatus.Resolved);

        conversation.Reopen();

        conversation.Status.Should().Be(ConversationStatus.Open);
    }

    [Fact]
    public void Reopen_FromOpen_ThrowsInvalidOperation()
    {
        var conversation = CreateConversation(ConversationStatus.Open);

        var act = () => conversation.Reopen();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Snooze_FromOpen_SetsStatusToSnoozed()
    {
        var conversation = CreateConversation(ConversationStatus.Open);
        var snoozeUntil = DateTime.UtcNow.AddHours(2);

        conversation.Snooze(snoozeUntil);

        conversation.Status.Should().Be(ConversationStatus.Snoozed);
        conversation.SnoozedUntil.Should().Be(snoozeUntil);
    }

    [Fact]
    public void Snooze_WithPastTime_ThrowsArgumentException()
    {
        var conversation = CreateConversation(ConversationStatus.Open);
        var pastTime = DateTime.UtcNow.AddHours(-1);

        var act = () => conversation.Snooze(pastTime);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Unsnooze_FromSnoozed_SetsStatusToOpen()
    {
        var conversation = CreateConversation(ConversationStatus.Snoozed);
        conversation.SnoozedUntil = DateTime.UtcNow.AddHours(1);

        conversation.Unsnooze();

        conversation.Status.Should().Be(ConversationStatus.Open);
        conversation.SnoozedUntil.Should().BeNull();
    }

    [Fact]
    public void Unsnooze_FromOpen_ThrowsInvalidOperation()
    {
        var conversation = CreateConversation(ConversationStatus.Open);

        var act = () => conversation.Unsnooze();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TogglePriority_FromNone_SetsToUrgent()
    {
        var conversation = CreateConversation();
        conversation.Priority = ConversationPriority.None;

        conversation.TogglePriority();

        conversation.Priority.Should().Be(ConversationPriority.Urgent);
    }

    [Fact]
    public void TogglePriority_FromUrgent_SetsToNone()
    {
        var conversation = CreateConversation();
        conversation.Priority = ConversationPriority.Urgent;

        conversation.TogglePriority();

        conversation.Priority.Should().Be(ConversationPriority.None);
    }

    [Fact]
    public void Mute_SetsMutedToTrue()
    {
        var conversation = CreateConversation();

        conversation.Mute();

        conversation.Muted.Should().BeTrue();
    }

    [Fact]
    public void Unmute_SetsMutedToFalse()
    {
        var conversation = CreateConversation();
        conversation.Muted = true;

        conversation.Unmute();

        conversation.Muted.Should().BeFalse();
    }

    [Fact]
    public void AssignTo_SetsAssigneeAndTeam()
    {
        var conversation = CreateConversation();

        conversation.AssignTo(5, 10);

        conversation.AssigneeId.Should().Be(5);
        conversation.TeamId.Should().Be(10);
    }

    [Fact]
    public void AssignTo_WithNull_UnassignsAgentAndTeam()
    {
        var conversation = CreateConversation();
        conversation.AssigneeId = 5;
        conversation.TeamId = 10;

        conversation.AssignTo(null, null);

        conversation.AssigneeId.Should().BeNull();
        conversation.TeamId.Should().BeNull();
    }

    [Fact]
    public void AddLabel_AddsLabelToCollection()
    {
        var conversation = CreateConversation();
        var label = new Label { Id = 1, AccountId = 1, Title = "bug", Color = "#ff0000" };

        conversation.AddLabel(label);

        conversation.Labels.Should().ContainSingle().Which.Id.Should().Be(1);
    }

    [Fact]
    public void AddLabel_DuplicateLabel_DoesNotAddAgain()
    {
        var conversation = CreateConversation();
        var label = new Label { Id = 1, AccountId = 1, Title = "bug", Color = "#ff0000" };

        conversation.AddLabel(label);
        conversation.AddLabel(label);

        conversation.Labels.Should().HaveCount(1);
    }

    [Fact]
    public void CanTransitionTo_ValidTransition_ReturnsTrue()
    {
        var conversation = CreateConversation(ConversationStatus.Open);

        conversation.CanTransitionTo(ConversationStatus.Resolved).Should().BeTrue();
        conversation.CanTransitionTo(ConversationStatus.Pending).Should().BeTrue();
        conversation.CanTransitionTo(ConversationStatus.Snoozed).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_SameStatus_ReturnsFalse()
    {
        var conversation = CreateConversation(ConversationStatus.Open);

        conversation.CanTransitionTo(ConversationStatus.Open).Should().BeFalse();
    }

    private static Conversation CreateConversation(ConversationStatus status = ConversationStatus.Open) => new()
    {
        Id = 1,
        AccountId = 10,
        InboxId = 1,
        ContactId = 2,
        Status = status,
        Priority = ConversationPriority.None,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
