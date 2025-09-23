using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;
using PatioVision.Core.Enums;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoService _service;
    private const int MaxPageSize = 100;

    public MotosController(MotoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna motos com paginação.
    /// </summary>
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
        var moto = _service.ObterPorId(id);
        return moto == null ? NotFound() : Ok(moto);
    }

    [HttpPost]
    public IActionResult Post([FromBody] Moto moto)
    {
        if (moto == null || string.IsNullOrEmpty(moto.Modelo))
            return BadRequest("Dados inválidos para criação.");

        var novaMoto = _service.Criar(moto);
        return CreatedAtAction(nameof(GetById), new { id = novaMoto.MotoId }, novaMoto);
    }

    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] Moto moto)
    {
        if (moto == null || id == Guid.Empty || id != moto.MotoId)
            return BadRequest("Dados inválidos para atualização.");

        var atualizada = _service.Atualizar(id, moto);
        return atualizada ? Ok(moto) : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Id inválido.");

        var removida = _service.Remover(id);
        return removida ? NoContent() : NotFound();
    }

    [HttpGet("status")]
    public IActionResult GetByStatus([FromQuery] string valor)
    {
        if (!Enum.TryParse<StatusMoto>(valor, true, out var status))
            return BadRequest("Status inválido. Use: Disponivel, EmManutencao ou Alugada.");

        var motos = _service.ObterPorStatus(status);
        return Ok(motos);
    }
}
