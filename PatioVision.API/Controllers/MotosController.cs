using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PatioVision.Core.Models;
using PatioVision.Service.Services;

namespace PatioVision.API.Controllers;

/// <summary>
/// Endpoints para gerenciamento de motos (listagem paginada, consulta por id, criação, atualização e remoção).
/// </summary>
/// <remarks>
/// Convenções:
/// - Rotas base: <c>/api/motos</c>
/// - HATEOAS retornado em <c>_links</c>
/// - Paginação com cabeçalhos <c>X-Total-Count</c> e <c>Link</c>
/// </remarks>
[ApiController]
[Route("api/motos")]
[Produces("application/json")]
public class MotosController : ControllerBase
{
    private readonly MotoService _service;
    public MotosController(MotoService service) => _service = service;

    /// <summary>
    /// Lista motos com paginação, filtro e ordenação.
    /// </summary>
    /// <remarks>
    /// Exemplos:
    /// - <c>GET /api/motos?pageNumber=1&amp;pageSize=10</c><br/>
    /// - <c>GET /api/motos?search=placa:ABC1D23</c><br/>
    /// - <c>GET /api/motos?sort=modelo,-ano</c> (ordena por <c>modelo</c> asc e <c>ano</c> desc)<br/><br/>
    /// Regras: <c>pageNumber &gt;= 1</c>, <c>1 &lt;= pageSize &lt;= 100</c>.
    /// </remarks>
    /// <param name="pageNumber">Número da página (padrão: 1).</param>
    /// <param name="pageSize">Tamanho da página, entre 1 e 100 (padrão: 10).</param>
    /// <param name="search">Filtro simples (ex.: <c>placa:ABC</c>, <c>modelo:CG</c>).</param>
    /// <param name="sort">Campos para ordenação (ex.: <c>modelo,-ano</c>). Padrão: <c>modelo</c>.</param>
    /// <response code="200">Lista paginada retornada com sucesso.</response>
    /// <response code="400">Parâmetros de paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sort = null)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest("pageNumber>=1 e 1<=pageSize<=100");

        var result = await _service.GetPagedAsync(pageNumber, pageSize, search, sort ?? "modelo");

        var items = result.Items.Select(x => new {
            data = x,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(x.MotoId), method = "GET" },
                ["update"] = new { href = SelfItem(x.MotoId), method = "PUT" },
                ["delete"] = new { href = SelfItem(x.MotoId), method = "DELETE" }
            }
        }).ToList();

        WritePaginationHeaders(result.PageNumber, result.PageSize, result.TotalItems, result.TotalPages, search, sort);

        var body = new
        {
            items,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize,
            totalItems = result.TotalItems,
            totalPages = result.TotalPages,
            _links = PageLinks(result.PageNumber, result.PageSize, result.TotalPages, search, sort)
        };

        return Ok(body);
    }

    /// <summary>
    /// Obtém detalhes de uma moto pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da moto.</param>
    /// <response code="200">Recurso encontrado.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var moto = _service.GetById(id);
        if (moto is null) return NotFound();

        var body = new
        {
            data = moto,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(id), method = "GET" },
                ["update"] = new { href = SelfItem(id), method = "PUT" },
                ["delete"] = new { href = SelfItem(id), method = "DELETE" },
                ["all"] = new { href = SelfPage(1, 10, null, null), method = "GET" }
            }
        };
        return Ok(body);
    }

    /// <summary>
    /// Cria uma nova moto.
    /// </summary>
    /// <remarks>
    /// **Exemplo de payload**:
    /// ```json
    /// {
    ///   "modelo": "CG 160",
    ///   "placa": "ABC1D23",
    ///   "ano": 2023
    /// }
    /// ```
    /// </remarks>
    /// <param name="moto">Dados da moto a ser criada.</param>
    /// <response code="201">Moto criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] Moto moto)
    {
        if (moto is null || string.IsNullOrWhiteSpace(moto.Modelo) || string.IsNullOrWhiteSpace(moto.Placa))
            return BadRequest("Dados inválidos.");

        var created = _service.Create(moto);

        var body = new
        {
            data = created,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(created.MotoId), method = "GET" },
                ["update"] = new { href = SelfItem(created.MotoId), method = "PUT" },
                ["delete"] = new { href = SelfItem(created.MotoId), method = "DELETE" },
                ["all"] = new { href = SelfPage(1, 10, null, null), method = "GET" }
            }
        };

        return CreatedAtAction(nameof(GetById), new { id = created.MotoId }, body);
    }

    /// <summary>
    /// Atualiza completamente uma moto existente.
    /// </summary>
    /// <param name="id">Identificador da moto.</param>
    /// <param name="moto">Dados atualizados da moto.</param>
    /// <response code="204">Atualizada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] Moto moto)
    {
        if (moto is null || moto.MotoId != id) return BadRequest("Dados inválidos.");
        var ok = _service.Update(id, moto);
        if (!ok) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Remove uma moto.
    /// </summary>
    /// <param name="id">Identificador da moto.</param>
    /// <response code="204">Excluída com sucesso.</response>
    /// <response code="400">ID inválido.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty) return BadRequest("ID inválido.");
        var ok = _service.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // Helpers
    private string SelfItem(Guid id) => $"{Request.Scheme}://{Request.Host}/api/motos/{id}";
    private string SelfPage(int pageNumber, int pageSize, string? search, string? sort)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
        var qs = new List<string> { $"pageNumber={pageNumber}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(sort)) qs.Add($"sort={Uri.EscapeDataString(sort)}");
        return $"{baseUrl}?{string.Join("&", qs)}";
    }
    private Dictionary<string, object> PageLinks(int pageNumber, int pageSize, int totalPages, string? search, string? sort)
    {
        var dict = new Dictionary<string, object>
        {
            ["self"] = new { href = SelfPage(pageNumber, pageSize, search, sort), method = "GET" }
        };
        if (pageNumber > 1)
            dict["prev"] = new { href = SelfPage(pageNumber - 1, pageSize, search, sort), method = "GET" };
        if (totalPages > 0)
        {
            dict["first"] = new { href = SelfPage(1, pageSize, search, sort), method = "GET" };
            dict["last"] = new { href = SelfPage(Math.Max(totalPages, 1), pageSize, search, sort), method = "GET" };
        }
        if (pageNumber < totalPages)
            dict["next"] = new { href = SelfPage(pageNumber + 1, pageSize, search, sort), method = "GET" };
        return dict;
    }
    private void WritePaginationHeaders(int pageNumber, int pageSize, int totalItems, int totalPages, string? search, string? sort)
    {
        Response.Headers["X-Total-Count"] = totalItems.ToString();

        var self = SelfPage(pageNumber, pageSize, search, sort);
        var first = SelfPage(1, pageSize, search, sort);
        var last = SelfPage(Math.Max(totalPages, 1), pageSize, search, sort);
        string? prev = pageNumber > 1 ? SelfPage(pageNumber - 1, pageSize, search, sort) : null;
        string? next = pageNumber < totalPages ? SelfPage(pageNumber + 1, pageSize, search, sort) : null;

        var parts = new List<string> {
            $"<{self}>; rel=\"self\"",
            $"<{first}>; rel=\"first\"",
            $"<{last}>; rel=\"last\""
        };
        if (prev is not null) parts.Add($"<{prev}>; rel=\"prev\"");
        if (next is not null) parts.Add($"<{next}>; rel=\"next\"");
        Response.Headers["Link"] = string.Join(", ", parts);
    }
}
