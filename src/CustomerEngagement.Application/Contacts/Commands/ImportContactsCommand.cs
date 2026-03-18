using MediatR;
using Microsoft.AspNetCore.Http;

namespace CustomerEngagement.Application.Contacts.Commands;

public record ImportContactsCommand(long AccountId, IFormFile File) : IRequest<ImportContactsResult>;

public record ImportContactsResult(int ImportedCount, int SkippedCount, IReadOnlyList<string> Errors);
