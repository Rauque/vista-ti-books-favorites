using Books.Api.Dtos;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly FavoritesService _service;

    public FavoritesController(FavoritesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var list = await _service.GetAsync(ct);
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddFavoriteRequestDto dto, CancellationToken ct)
    {
        var (ok, statusCode, message, favorite) = await _service.AddAsync(dto, ct);

        if (!ok)
            return StatusCode(statusCode, new { message });

        return StatusCode(201, favorite);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        var deleted = await _service.DeleteAsync(id, ct);
        if (!deleted) return NotFound(new { message = "Favorito no encontrado." });

        return NoContent();
    }
}
