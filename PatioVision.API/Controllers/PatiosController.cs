using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PatioVision.Core.Models;
using PatioVision.Service.Services;

namespace PatioVision.API.Controllers;

/// <summary>
/// Endpoints para gerenciamento de p�tios.
/// </summary>
/// <remarks>
/// Rotas base: <c>/api/patios</c>. Suporta pagina��o, HATEOAS e cabe�alhos de navega��o.
/// </remarks>
[ApiController]
[Route("api/patios")]
[Produces("application/json")]
public class PatioController : ControllerBase
{
    private readonly PatioService _service;
    public PatioController(PatioService service) => _service = service;

    /// <summary>
    /// Lista p�tios com pagina��o, filtro e ordena��o.
    /// </summary>
    /// <remarks>
    /// Ex.: <c>GET /api/patios?pageNumber=1&amp;pageSize=10&amp;search=nome:Centro&amp;sort=-dtcadastro</c><br/>
    /// Regras: <c>pageNumber &gt;= 1</c>, <c>1 &lt;= pageSize &lt;= 100</c>. Padr�o de sort: <c>-dtcadastro</c>.
    /// </remarks>
    /// <param name="pageNumber">N�mero da p�gina (padr�o: 1).</param>
    /// <param name="pageSize">Tamanho da p�gina, entre 1 e 100 (padr�o: 10).</param>
    /// <param name="search">Filtro simples (ex.: <c>nome:Centro</c>).</param>
    /// <param name="sort">Ordena��o (ex.: <c>nome,-cidade</c>). Padr�o: <c>-dtcadastro</c>.</param>
    /// <response code="200">Lista paginada retornada.</response>
    /// <response code="400">Par�metros de pagina��o inv�lidos.</response>
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

        var result = await _service.GetPagedAsync(pageNumber, pageSize, search, sort ?? "-dtcadastro");

        var items = result.Items.Select(x => new {
            data = x,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(x.PatioId), method = "GET" },
                ["update"] = new { href = SelfItem(x.PatioId), method = "PUT" },
                ["delete"] = new { href = SelfItem(x.PatioId), method = "DELETE" }
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
    /// Obt�m um p�tio pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do p�tio.</param>
    /// <response code="200">Recurso encontrado.</response>
    /// <response code="404">P�tio n�o encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var patio = _service.GetById(id);
        if (patio is null) return NotFound();

        var body = new
        {
            data = patio,
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
    /// Cria um novo p�tio.
    /// </summary>
    /// <remarks>
    /// **Exemplo**:
    /// ```json
    /// { "nome": "P�tio Centro", "cidade": "S�o Paulo", "capacidade": 300 }
    /// ```
    /// </remarks>
    /// <param name="patio">Dados do p�tio.</param>
    /// <response code="201">Criado com sucesso.</response>
    /// <response code="400">Dados inv�lidos.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] Patio patio)
    {
        if (patio is null || string.IsNullOrWhiteSpace(patio.Nome))
            return BadRequest("Dados inv�lidos.");

        var created = _service.Create(patio);

        var body = new
        {
            data = created,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(created.PatioId), method = "GET" },
                ["update"] = new { href = SelfItem(created.PatioId), method = "PUT" },
                ["delete"] = new { href = SelfItem(created.PatioId), method = "DELETE" },
                ["all"] = new { href = SelfPage(1, 10, null, null), method = "GET" }
            }
        };

        return CreatedAtAction(nameof(GetById), new { id = created.PatioId }, body);
    }

    /// <summary>
    /// Atualiza completamente um p�tio existente.
    /// </summary>
    /// <param name="id">Identificador do p�tio.</param>
    /// <param name="patio">Dados atualizados do p�tio.</param>
    /// <response code="204">Atualizado com sucesso.</response>
    /// <response code="400">Dados inv�lidos.</response>
    /// <response code="404">P�tio n�o encontrado.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] Patio patio)
    {
        if (patio is null || patio.PatioId != id) return BadRequest("Dados inv�lidos.");
        var ok = _service.Update(id, patio);
        if (!ok) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Remove um p�tio.
    /// </summary>
    /// <param name="id">Identificador do p�tio.</param>
    /// <response code="204">Exclu�do com sucesso.</response>
    /// <response code="400">ID inv�lido.</response>
    /// <response code="404">P�tio n�o encontrado.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty) return BadRequest("ID inv�lido.");
        var ok = _service.Delete(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ===== Helpers =====
    private string SelfItem(Guid id) => $"{Request.Scheme}://{Request.Host}/api/patios/{id}";
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
