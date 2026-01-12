namespace Books.Api.Dtos;

public class AddFavoriteRequestDto
{
    public string ExternalId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public List<string> Authors { get; set; } = new();
    public int? FirstPublishYear { get; set; }
    public int? CoverId { get; set; }
    public string? CoverUrl { get; set; }
}
