namespace Books.Api.Dtos;

public record BookSearchItemDto(
    string ExternalId,
    string Title,
    List<string> Authors,
    int? FirstPublishYear,
    int? CoverId,
    string? CoverUrl
);
