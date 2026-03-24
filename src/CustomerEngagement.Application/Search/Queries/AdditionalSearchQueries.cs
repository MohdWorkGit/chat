using MediatR;

namespace CustomerEngagement.Application.Search.Queries;

public record GlobalSearchQuery(long AccountId, string Query, int Page, int PageSize) : IRequest<object>;

public record SearchConversationsQuery(long AccountId, string Query, int Page, int PageSize) : IRequest<object>;

public record SearchContactsQuery(long AccountId, string Query, int Page, int PageSize) : IRequest<object>;

public record SearchMessagesQuery(long AccountId, string Query, int Page, int PageSize) : IRequest<object>;
