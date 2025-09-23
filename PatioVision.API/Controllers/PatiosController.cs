using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatiosController : ControllerBase
{
    private readonly PatioService _service;
    private const int MaxPageSize = 100;

    public PatiosController(PatioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que 0.");

        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        var result = await _service.ObterPaginadoAsync(pageNumber, pageSize, ct);

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(new
        {
            result.TotalItems,
            result.PageNumber,
            result.PageSize,
            result.TotalPages
        }));

        return Ok(result.Items);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var patio = _service.ObterPorId(id);
        return patio is null ? NotFound() : Ok(patio);
    }

    [HttpPost]
    public IActionResult Post([FromBody] Patio patio)
    {
        if (patio == null) return BadRequest("Dados inválidos");
        var novo = _service.Criar(patio);
        return CreatedAtAction(nameof(GetById), new { id = novo.PatioId }, novo);
    }

    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] Patio patio)
    {
        if (patio == null || id != patio.PatioId) return BadRequest();
        var atualizado = _service.Atualizar(id, patio);
        return atualizado ? Ok(patio) : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var removido = _service.Remover(id);
        return removido ? NoContent() : NotFound();
    }
}
