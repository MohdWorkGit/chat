using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerEngagement.Core.Entities;
using CustomerEngagement.Enterprise.Captain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using FluentAssertions;

namespace CustomerEngagement.Tests.Enterprise;

public class CopilotServiceTests
{
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<ILogger<CopilotService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly CopilotService _sut;

    public CopilotServiceTests()
    {
        _dbContextMock = new Mock<DbContext>();
        _loggerMock = new Mock<ILogger<CopilotService>>();
        _httpHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:11434")
        };

        // Setup DbContext to return empty messages
        var messages = new List<Message>().AsQueryable();
        var mockSet = new Mock<DbSet<Message>>();
        mockSet.As<IQueryable<Message>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Message>(messages.Provider));
        mockSet.As<IQueryable<Message>>().Setup(m => m.Expression).Returns(messages.Expression);
        mockSet.As<IQueryable<Message>>().Setup(m => m.ElementType).Returns(messages.ElementType);
        mockSet.As<IQueryable<Message>>().Setup(m => m.GetEnumerator()).Returns(messages.GetEnumerator());
        mockSet.As<IAsyncEnumerable<Message>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Message>(messages.GetEnumerator()));

        _dbContextMock.Setup(c => c.Set<Message>()).Returns(mockSet.Object);

        _sut = new CopilotService(
            _dbContextMock.Object,
            _httpClient,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RewriteMessage_ReturnsRewrittenText()
    {
        var ollamaResponse = JsonSerializer.Serialize(new { response = "Dear customer, we appreciate your patience." });

        SetupHttpResponse(HttpStatusCode.OK, ollamaResponse);

        var result = await _sut.RewriteAsync("Hey, wait a moment.", "formal");

        result.Should().NotBeNull();
        result.OriginalText.Should().Be("Hey, wait a moment.");
        result.RewrittenText.Should().Be("Dear customer, we appreciate your patience.");
        result.Tone.Should().Be("formal");
    }

    [Fact]
    public async Task SummarizeConversation_ReturnsSummary()
    {
        var summaryJson = JsonSerializer.Serialize(new
        {
            response = JsonSerializer.Serialize(new
            {
                summary = "Customer reported a billing issue. Agent resolved by issuing a refund.",
                keyPoints = new[] { "Billing issue", "Refund issued" }
            })
        });

        SetupHttpResponse(HttpStatusCode.OK, summaryJson);

        var result = await _sut.SummarizeAsync(1);

        result.Should().NotBeNull();
        result.Summary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SuggestReply_ReturnsSuggestion()
    {
        var ollamaResponse = JsonSerializer.Serialize(new
        {
            response = "Thank you for reaching out! Let me look into this issue for you."
        });

        SetupHttpResponse(HttpStatusCode.OK, ollamaResponse);

        var result = await _sut.SuggestReplyAsync(1);

        result.Should().NotBeNull();
        result.Content.Should().NotBeNullOrEmpty();
        result.Confidence.Should().Be(0.85);
    }

    [Fact]
    public async Task SuggestLabels_ReturnsLabelSuggestions()
    {
        var labelsJson = JsonSerializer.Serialize(new[] { "billing", "refund", "urgent" });
        var ollamaResponse = JsonSerializer.Serialize(new { response = labelsJson });

        SetupHttpResponse(HttpStatusCode.OK, ollamaResponse);

        var result = await _sut.SuggestLabelsAsync(1);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RewriteMessage_WhenOllamaFails_ReturnsErrorMessage()
    {
        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _sut.RewriteAsync("Hello", "formal");

        result.Should().NotBeNull();
        result.RewrittenText.Should().Be("Unable to generate suggestion at this time.");
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        _httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json")
            });
    }
}

// Async query provider helpers for EF Core mocking
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression) =>
        new TestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) =>
        new TestAsyncEnumerable<TElement>(expression);

    public object? Execute(System.Linq.Expressions.Expression expression) =>
        _inner.Execute(expression);

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) =>
        _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: [typeof(System.Linq.Expressions.Expression)])!
            .MakeGenericMethod(resultType)
            .Invoke(this, [expression]);

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, [executionResult])!;
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

    public T Current => _inner.Current;
}
