namespace Books.Api.Dtos;

public record FavoriteDto(
    int Id,
    string ExternalId,
    string Title,
    List<string> Authors,
    int? FirstPublishYear,
    int? CoverId,
    string? CoverUrl,
    DateTime CreatedAt
);
