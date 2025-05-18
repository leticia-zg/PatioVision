using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;
using System;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispositivosController : ControllerBase
{
    private readonly DispositivoService _service;

    public DispositivosController(DispositivoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna todos os dispositivos IoT cadastrados.
    /// </summary>
    [HttpGet]
    public IActionResult Get() => Ok(_service.ObterTodos());

    /// <summary>
    /// Retorna um dispositivo por ID.
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var disp = _service.ObterPorId(id);
        return disp is null ? NotFound() : Ok(disp);
    }

    /// <summary>
    /// Cria um novo dispositivo.
    /// </summary>
    [HttpPost]
    public IActionResult Post([FromBody] DispositivoIoT dispositivo)
    {
        if (dispositivo == null) return BadRequest("Dados inválidos");
        var novo = _service.Criar(dispositivo);
        return CreatedAtAction(nameof(GetById), new { id = novo.DispositivoIotId }, novo);
    }

    /// <summary>
    /// Atualiza um dispositivo existente.
    /// </summary>
    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] DispositivoIoT dispositivo)
    {
        if (dispositivo == null || id != dispositivo.DispositivoIotId) return BadRequest();
        var atualizado = _service.Atualizar(id, dispositivo);
        return atualizado ? Ok(dispositivo) : NotFound();
    }

    /// <summary>
    /// Remove um dispositivo pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var removido = _service.Remover(id);
        return removido ? NoContent() : NotFound();
    }
}
