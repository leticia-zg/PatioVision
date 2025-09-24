using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/dispositivos")]
public class DispositivosController : ControllerBase
{
    private readonly DispositivoService _service;
    public DispositivosController(DispositivoService service) => _service = service;

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

        // default alinhado com o service: "-ultimaatualizacao"
        var result = await _service.GetPagedAsync(pageNumber, pageSize, search, sort ?? "-ultimaatualizacao");

        var items = result.Items.Select(x => new {
            data = x,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(x.DispositivoIotId), method = "GET" },
                ["update"] = new { href = SelfItem(x.DispositivoIotId), method = "PUT" },
                ["delete"] = new { href = SelfItem(x.DispositivoIotId), method = "DELETE" }
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var disp = _service.GetById(id);
        if (disp is null) return NotFound();

        var body = new
        {
            data = disp,
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

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] DispositivoIoT disp)
    {
        if (disp is null) return BadRequest("Dados inválidos.");

        var created = _service.Create(disp);

        var body = new
        {
            data = created,
            _links = new Dictionary<string, object>
            {
                ["self"] = new { href = SelfItem(created.DispositivoIotId), method = "GET" },
                ["update"] = new { href = SelfItem(created.DispositivoIotId), method = "PUT" },
                ["delete"] = new { href = SelfItem(created.DispositivoIotId), method = "DELETE" },
                ["all"] = new { href = SelfPage(1, 10, null, null), method = "GET" }
            }
        };

        return CreatedAtAction(nameof(GetById), new { id = created.DispositivoIotId }, body);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] DispositivoIoT disp)
    {
        if (disp is null || disp.DispositivoIotId != id) return BadRequest("Dados inválidos.");
        var ok = _service.Update(id, disp);
        if (!ok) return NotFound();
        return NoContent();
    }

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
    private string SelfItem(Guid id) => $"{Request.Scheme}://{Request.Host}/api/dispositivos/{id}";
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
