using Books.Api.Clients;
using Books.Api.Dtos;

namespace Books.Api.Services;

public class BooksService
{
    private readonly OpenLibraryClient _client;

    public BooksService(OpenLibraryClient client)
    {
        _client = client;
    }

    public async Task<List<BookSearchItemDto>> SearchAsync(string query, int page, int limit, CancellationToken ct)
    {
        var res = await _client.SearchAsync(query, page, limit, ct);

        return res.Docs
            .Where(d => !string.IsNullOrWhiteSpace(d.Key) && !string.IsNullOrWhiteSpace(d.Title))
            .Select(d =>
            {
                var coverUrl = d.CoverId.HasValue
                    ? $"https://covers.openlibrary.org/b/id/{d.CoverId.Value}-M.jpg"
                    : null;

                return new BookSearchItemDto(
                    ExternalId: d.Key!,
                    Title: d.Title!,
                    Authors: d.AuthorName ?? new List<string>(),
                    FirstPublishYear: d.FirstPublishYear,
                    CoverId: d.CoverId,
                    CoverUrl: coverUrl
                );
            })
            .ToList();
    }
}
