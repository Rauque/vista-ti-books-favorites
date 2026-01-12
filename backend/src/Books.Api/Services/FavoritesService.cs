using Books.Api.Dtos;
using Books.Api.Repositories;
using System.Data;

namespace Books.Api.Services;

public class FavoritesService
{
    private const int UserId = 1; // simplificación para la prueba
    private readonly FavoritesRepository _repo;

    public FavoritesService(FavoritesRepository repo)
    {
        _repo = repo;
    }

    public Task<List<FavoriteDto>> GetAsync(CancellationToken ct)
        => _repo.GetByUserAsync(UserId, ct);

    public async Task<(bool ok, int statusCode, string? message, FavoriteDto? favorite)> AddAsync(AddFavoriteRequestDto dto, CancellationToken ct)
    {
        if (dto is null)
            return (false, 400, "Body requerido.", null);

        if (string.IsNullOrWhiteSpace(dto.ExternalId) ||
            string.IsNullOrWhiteSpace(dto.Title) ||
            dto.Authors is null || dto.Authors.Count == 0)
        {
            return (false, 400, "ExternalId, Title y Authors son requeridos.", null);
        }

        var externalId = dto.ExternalId.Trim();
        var title = dto.Title.Trim();

        var authorsList = dto.Authors
            .Select(a => a?.Trim())
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .ToList();

        if (authorsList.Count == 0)
            return (false, 400, "Authors debe tener al menos 1 autor.", null);

        var authorsCsv = string.Join(", ", authorsList);

        try
        {
            var (id, createdAt) = await _repo.InsertAsync(
                userId: UserId,
                externalId: externalId,
                title: title,
                authorsCsv: authorsCsv,
                firstPublishYear: dto.FirstPublishYear,
                coverId: dto.CoverId,
                coverUrl: dto.CoverUrl,
                ct: ct
            );

            var favorite = new FavoriteDto(
                Id: id,
                ExternalId: externalId,
                Title: title,
                Authors: authorsList,
                FirstPublishYear: dto.FirstPublishYear,
                CoverId: dto.CoverId,
                CoverUrl: dto.CoverUrl,
                CreatedAt: createdAt
            );

            return (true, 201, null, favorite);
        }
        catch (DuplicateNameException)
        {
            return (false, 409, "El libro ya está en favoritos.", null);
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
        => await _repo.DeleteAsync(UserId, id, ct);
}
