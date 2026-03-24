using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.Notifications;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<Notification>> _notificationRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _notificationRepoMock = new Mock<IRepository<Notification>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new NotificationService(
            _notificationRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesNotificationAndReturnsDto()
    {
        var request = new CreateNotificationRequest
        {
            AccountId = 1,
            UserId = 10,
            NotificationType = "conversation_created",
            PrimaryActorType = "User",
            PrimaryActorId = 5,
            SecondaryActorType = "Conversation",
            SecondaryActorId = 100
        };

        _notificationRepoMock.Setup(r => r.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification n, CancellationToken _) => n);

        var result = await _sut.CreateAsync(request);

        result.Should().NotBeNull();
        result.NotificationType.Should().Be("conversation_created");
        result.PrimaryActorType.Should().Be("User");
        result.PrimaryActorId.Should().Be(5);
        result.SecondaryActorType.Should().Be("Conversation");
        result.SecondaryActorId.Should().Be(100);
        result.IsRead.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkReadAsync_WhenNotificationExists_MarksAsRead()
    {
        var notification = CreateTestNotification(1);
        _notificationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        await _sut.MarkReadAsync(1);

        notification.ReadAt.Should().NotBeNull();
        notification.IsRead.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.UpdateAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkReadAsync_WhenNotificationNotFound_Throws()
    {
        _notificationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var act = () => _sut.MarkReadAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsNotificationsForUser()
    {
        var notifications = new List<Notification>
        {
            CreateTestNotification(1),
            CreateTestNotification(2),
            CreateTestNotification(3)
        };

        _notificationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications.AsReadOnly());

        var result = await _sut.GetByUserAsync(10, 1);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task MarkAllReadAsync_MarksAllUnreadNotificationsAsRead()
    {
        var notifications = new List<Notification>
        {
            CreateTestNotification(1),
            CreateTestNotification(2)
        };

        _notificationRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifications.AsReadOnly());

        await _sut.MarkAllReadAsync(10, 1);

        foreach (var notification in notifications)
        {
            notification.ReadAt.Should().NotBeNull();
            notification.IsRead.Should().BeTrue();
        }

        _notificationRepoMock.Verify(
            r => r.UpdateAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotificationExists_DeletesNotification()
    {
        var notification = CreateTestNotification(1);
        _notificationRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);

        await _sut.DeleteAsync(1);

        _notificationRepoMock.Verify(r => r.DeleteAsync(notification, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotificationNotFound_Throws()
    {
        _notificationRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    private static Notification CreateTestNotification(int id) => new()
    {
        Id = id,
        AccountId = 1,
        UserId = 10,
        NotificationType = "conversation_created",
        PrimaryActorType = "User",
        PrimaryActorId = 5,
        SecondaryActorType = "Conversation",
        SecondaryActorId = 100,
        ReadAt = null,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
