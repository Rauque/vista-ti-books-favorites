using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Books.Api.Clients;

public class OpenLibraryClient
{
    private readonly HttpClient _http;

    public OpenLibraryClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<SearchResponse> SearchAsync(string query, int page, int limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("query is required", nameof(query));

        page = page <= 0 ? 1 : page;
        limit = (limit is <= 0 or > 50) ? 20 : limit;

        var fields = "key,title,author_name,first_publish_year,cover_i";
        var url = $"search.json?q={Uri.EscapeDataString(query)}&fields={fields}&page={page}&limit={limit}";

        using var resp = await _http.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException($"OpenLibrary error {(int)resp.StatusCode}");

        var data = await resp.Content.ReadFromJsonAsync<SearchResponse>(cancellationToken: ct);
        return data ?? new SearchResponse();
    }

    public class SearchResponse
    {
        [JsonPropertyName("docs")]
        public List<SearchDoc> Docs { get; set; } = new();
    }

    public class SearchDoc
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("author_name")]
        public List<string>? AuthorName { get; set; }

        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }

        [JsonPropertyName("cover_i")]
        public int? CoverId { get; set; }
    }
}
