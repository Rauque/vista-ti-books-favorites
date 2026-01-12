using Books.Api.Dtos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Books.Api.Repositories;

public class FavoritesRepository
{
    private readonly string _connectionString;

    public FavoritesRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing connection string: ConnectionStrings:DefaultConnection");
    }

    public async Task<List<FavoriteDto>> GetByUserAsync(int userId, CancellationToken ct)
    {
        const string sql = @"
SELECT Id, ExternalId, Title, Authors, FirstPublishYear, CoverId, CoverUrl, CreatedAt
FROM dbo.Favorites
WHERE UserId = @UserId
ORDER BY CreatedAt DESC;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        var result = new List<FavoriteDto>();

        while (await reader.ReadAsync(ct))
        {
            var authorsCsv = reader.GetString(reader.GetOrdinal("Authors"));
            var authors = authorsCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            result.Add(new FavoriteDto(
                Id: reader.GetInt32(reader.GetOrdinal("Id")),
                ExternalId: reader.GetString(reader.GetOrdinal("ExternalId")),
                Title: reader.GetString(reader.GetOrdinal("Title")),
                Authors: authors,
                FirstPublishYear: reader.IsDBNull(reader.GetOrdinal("FirstPublishYear")) ? null : reader.GetInt32(reader.GetOrdinal("FirstPublishYear")),
                CoverId: reader.IsDBNull(reader.GetOrdinal("CoverId")) ? null : reader.GetInt32(reader.GetOrdinal("CoverId")),
                CoverUrl: reader.IsDBNull(reader.GetOrdinal("CoverUrl")) ? null : reader.GetString(reader.GetOrdinal("CoverUrl")),
                CreatedAt: reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            ));
        }

        return result;
    }

    public async Task<(int id, DateTime createdAt)> InsertAsync(
        int userId,
        string externalId,
        string title,
        string authorsCsv,
        int? firstPublishYear,
        int? coverId,
        string? coverUrl,
        CancellationToken ct)
    {
        const string sql = @"
INSERT INTO dbo.Favorites (UserId, ExternalId, Title, Authors, FirstPublishYear, CoverId, CoverUrl)
OUTPUT inserted.Id, inserted.CreatedAt
VALUES (@UserId, @ExternalId, @Title, @Authors, @FirstPublishYear, @CoverId, @CoverUrl);";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
        cmd.Parameters.Add("@ExternalId", SqlDbType.NVarChar, 200).Value = externalId;
        cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 500).Value = title;
        cmd.Parameters.Add("@Authors", SqlDbType.NVarChar, 2000).Value = authorsCsv;

        cmd.Parameters.Add("@FirstPublishYear", SqlDbType.Int).Value = (object?)firstPublishYear ?? DBNull.Value;
        cmd.Parameters.Add("@CoverId", SqlDbType.Int).Value = (object?)coverId ?? DBNull.Value;
        cmd.Parameters.Add("@CoverUrl", SqlDbType.NVarChar, 500).Value = (object?)coverUrl ?? DBNull.Value;

        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);

            var id = reader.GetInt32(0);
            var createdAt = reader.GetDateTime(1);

            return (id, createdAt);
        }
        catch (SqlException ex) when (ex.Number is 2601 or 2627)
        {
            // Unique index (UserId, ExternalId)
            throw new DuplicateNameException("Duplicate favorite for this user.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int userId, int favoriteId, CancellationToken ct)
    {
        const string sql = @"
DELETE FROM dbo.Favorites
WHERE Id = @Id AND UserId = @UserId;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = favoriteId;
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        return affected > 0;
    }
}
