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
public class DispositivosController : ControllerBase
{
    private readonly DispositivoService _service;
    private const int MaxPageSize = 100;

    public DispositivosController(DispositivoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna dispositivos IoT com paginação.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        if (pageNumber <= 0 || pageSize <= 0)
            return BadRequest("pageNumber e pageSize devem ser maiores que 0.");

        if (pageSize > MaxPageSize)
            pageSize = MaxPageSize;

        var result = await _service.ObterPaginadoAsync(pageNumber, pageSize, ct);

        // Adiciona os metadados no header
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
        var disp = _service.ObterPorId(id);
        return disp is null ? NotFound() : Ok(disp);
    }

    [HttpPost]
    public IActionResult Post([FromBody] DispositivoIoT dispositivo)
    {
        if (dispositivo == null) return BadRequest("Dados inválidos");
        var novo = _service.Criar(dispositivo);
        return CreatedAtAction(nameof(GetById), new { id = novo.DispositivoIotId }, novo);
    }

    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] DispositivoIoT dispositivo)
    {
        if (dispositivo == null || id != dispositivo.DispositivoIotId) return BadRequest();
        var atualizado = _service.Atualizar(id, dispositivo);
        return atualizado ? Ok(dispositivo) : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var removido = _service.Remover(id);
        return removido ? NoContent() : NotFound();
    }
}
