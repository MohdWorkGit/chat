using CustomerEngagement.Application.DTOs;
using CustomerEngagement.Application.Services.HelpCenter;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Interfaces;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerEngagement.Application.Tests.Services;

public class PortalServiceTests
{
    private readonly Mock<IRepository<Portal>> _portalRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly PortalService _sut;

    public PortalServiceTests()
    {
        _portalRepoMock = new Mock<IRepository<Portal>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new PortalService(_portalRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPortalExists_ReturnsDto()
    {
        var portal = CreateTestPortal(1);
        _portalRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portal);

        var result = await _sut.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Portal");
        result.Slug.Should().Be("test-portal");
    }

    [Fact]
    public async Task GetByIdAsync_WhenPortalNotFound_ReturnsNull()
    {
        _portalRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portal?)null);

        var result = await _sut.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WhenPortalExists_ReturnsDto()
    {
        var portal = CreateTestPortal(1);
        _portalRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Portal> { portal });

        var result = await _sut.GetBySlugAsync("test-portal");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("test-portal");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ReturnsNull()
    {
        _portalRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Portal>());

        var result = await _sut.GetBySlugAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByAccountAsync_ReturnsPortalsForAccount()
    {
        var portals = new List<Portal>
        {
            CreateTestPortal(1),
            CreateTestPortal(2)
        };
        _portalRepoMock.Setup(r => r.ListAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(portals);

        var result = await _sut.GetByAccountAsync(10);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_CreatesPortalAndSaves()
    {
        var request = new CreatePortalRequest
        {
            Name = "New Portal",
            Slug = "new-portal",
            Color = "#FF0000",
            HeaderText = "Welcome"
        };

        _portalRepoMock.Setup(r => r.AddAsync(It.IsAny<Portal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portal p, CancellationToken _) => p);

        var result = await _sut.CreateAsync(10, request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Portal");
        result.Slug.Should().Be("new-portal");
        result.Color.Should().Be("#FF0000");
        result.IsArchived.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenPortalExists_UpdatesFields()
    {
        var portal = CreateTestPortal(1);
        _portalRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portal);

        var request = new UpdatePortalRequest
        {
            Name = "Updated Name",
            Color = "#00FF00"
        };

        var result = await _sut.UpdateAsync(1, request);

        result.Name.Should().Be("Updated Name");
        result.Color.Should().Be("#00FF00");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenPortalNotFound_Throws()
    {
        _portalRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portal?)null);

        var act = () => _sut.UpdateAsync(999, new UpdatePortalRequest { Name = "X" });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task DeleteAsync_WhenPortalExists_Deletes()
    {
        var portal = CreateTestPortal(1);
        _portalRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portal);

        await _sut.DeleteAsync(1);

        _portalRepoMock.Verify(r => r.DeleteAsync(portal, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPortalNotFound_Throws()
    {
        _portalRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Portal?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    private static Portal CreateTestPortal(int id) => new()
    {
        Id = id,
        AccountId = 10,
        Name = "Test Portal",
        Slug = "test-portal",
        Color = "#1F93FF",
        HeaderText = "Help Center",
        Archived = false,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
