using Books.Api.Dtos;
using Books.Api.Repositories;
using Books.Api.Services;
using FluentAssertions;
using Moq;
using System.Data;

namespace Books.Api.Tests;

public class FavoritesServiceTests
{
    [Fact]
    public async Task AddAsync_Returns400_WhenExternalIdMissing()
    {
        var repo = new Mock<IFavoritesRepository>(MockBehavior.Strict);
        var svc = new FavoritesService(repo.Object);

        var dto = new AddFavoriteRequestDto
        {
            ExternalId = " ",
            Title = "Title",
            Authors = new List<string> { "Author" }
        };

        var result = await svc.AddAsync(dto, CancellationToken.None);

        result.ok.Should().BeFalse();
        result.statusCode.Should().Be(400);
    }

    [Fact]
    public async Task AddAsync_Returns409_WhenDuplicate()
    {
        var repo = new Mock<IFavoritesRepository>(MockBehavior.Strict);
        repo.Setup(r => r.InsertAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        var svc = new FavoritesService(repo.Object);

        var dto = new AddFavoriteRequestDto
        {
            ExternalId = "/works/OL82563W",
            Title = "Harry Potter",
            Authors = new List<string> { "J. K. Rowling" }
        };

        var result = await svc.AddAsync(dto, CancellationToken.None);

        result.ok.Should().BeFalse();
        result.statusCode.Should().Be(409);

        repo.VerifyAll();
    }

    [Fact]
    public async Task AddAsync_Returns201_AndFavorite_WhenOk()
    {
        var now = new DateTime(2026, 01, 12, 0, 0, 0, DateTimeKind.Utc);

        var repo = new Mock<IFavoritesRepository>(MockBehavior.Strict);
        repo.Setup(r => r.InsertAsync(
                1,
                "/works/OL82563W",
                "Harry Potter",
                "J. K. Rowling",
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((id: 123, createdAt: now));

        var svc = new FavoritesService(repo.Object);

        var dto = new AddFavoriteRequestDto
        {
            ExternalId = " /works/OL82563W ",
            Title = " Harry Potter ",
            Authors = new List<string?> { " J. K. Rowling " }
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList()
        };

        var result = await svc.AddAsync(dto, CancellationToken.None);

        result.ok.Should().BeTrue();
        result.statusCode.Should().Be(201);
        result.favorite.Should().NotBeNull();
        result.favorite!.Id.Should().Be(123);
        result.favorite.ExternalId.Should().Be("/works/OL82563W");
        result.favorite.Title.Should().Be("Harry Potter");
        result.favorite.Authors.Should().ContainSingle().Which.Should().Be("J. K. Rowling");

        repo.VerifyAll();
    }
}
