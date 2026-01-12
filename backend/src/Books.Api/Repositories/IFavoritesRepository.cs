using Books.Api.Dtos;

namespace Books.Api.Repositories;

public interface IFavoritesRepository
{
    Task<List<FavoriteDto>> GetByUserAsync(int userId, CancellationToken ct);

    Task<(int id, DateTime createdAt)> InsertAsync(
        int userId,
        string externalId,
        string title,
        string authorsCsv,
        int? firstPublishYear,
        int? coverId,
        string? coverUrl,
        CancellationToken ct);

    Task<bool> DeleteAsync(int userId, int favoriteId, CancellationToken ct);
}
