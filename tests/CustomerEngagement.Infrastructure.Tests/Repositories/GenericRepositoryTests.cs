using CustomerEngagement.Core.Entities;
using CustomerEngagement.Core.Enums;
using CustomerEngagement.Infrastructure.Persistence;
using CustomerEngagement.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CustomerEngagement.Infrastructure.Tests.Repositories;

public class GenericRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly GenericRepository<Contact> _repository;

    public GenericRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new GenericRepository<Contact>(_context);
    }

    [Fact]
    public async Task AddAsync_AddsEntityToDatabase()
    {
        var contact = CreateTestContact("John", "john@example.com");

        var result = await _repository.AddAsync(contact);
        await _context.SaveChangesAsync();

        result.Should().NotBeNull();
        var saved = await _context.Contacts.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("John");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntity()
    {
        var contact = CreateTestContact("Jane", "jane@example.com");
        await _repository.AddAsync(contact);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(contact.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Jane");
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _repository.AddAsync(CreateTestContact("Bob", "bob@example.com"));
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindAsync_FiltersByPredicate()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _repository.AddAsync(CreateTestContact("Bob", "bob@example.com"));
        await _context.SaveChangesAsync();

        var result = await _repository.FindAsync(c => c.Name == "Alice");

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public async Task FindOneAsync_ReturnsFirstMatch()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _repository.AddAsync(CreateTestContact("Bob", "bob@example.com"));
        await _context.SaveChangesAsync();

        var result = await _repository.FindOneAsync(c => c.Email == "bob@example.com");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Bob");
    }

    [Fact]
    public async Task AnyAsync_ReturnsTrueWhenExists()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _context.SaveChangesAsync();

        var result = await _repository.AnyAsync(c => c.Name == "Alice");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AnyAsync_ReturnsFalseWhenNotExists()
    {
        var result = await _repository.AnyAsync(c => c.Name == "NonExistent");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _repository.AddAsync(CreateTestContact("Bob", "bob@example.com"));
        await _context.SaveChangesAsync();

        var count = await _repository.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ReturnsFilteredCount()
    {
        await _repository.AddAsync(CreateTestContact("Alice", "alice@example.com"));
        await _repository.AddAsync(CreateTestContact("Bob", "bob@example.com"));
        await _context.SaveChangesAsync();

        var count = await _repository.CountAsync(c => c.Name == "Alice");

        count.Should().Be(1);
    }

    [Fact]
    public async Task Remove_DeletesEntity()
    {
        var contact = CreateTestContact("Alice", "alice@example.com");
        await _repository.AddAsync(contact);
        await _context.SaveChangesAsync();

        _repository.Remove(contact);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(contact.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResults()
    {
        for (int i = 1; i <= 10; i++)
        {
            await _repository.AddAsync(CreateTestContact($"User{i}", $"user{i}@example.com"));
        }
        await _context.SaveChangesAsync();

        var page1 = await _repository.GetPagedAsync(1, 3);
        var page2 = await _repository.GetPagedAsync(2, 3);

        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static Contact CreateTestContact(string name, string email) => new()
    {
        AccountId = 1,
        Name = name,
        Email = email,
        ContactType = ContactType.Customer,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
}
