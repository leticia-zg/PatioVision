using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;
using System;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatiosController : ControllerBase
{
    private readonly PatioService _service;

    public PatiosController(PatioService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna todos os pátios cadastrados.
    /// </summary>
    [HttpGet]
    public IActionResult Get() => Ok(_service.ObterTodos());

    /// <summary>
    /// Retorna um pátio por ID.
    /// </summary>
    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var patio = _service.ObterPorId(id);
        return patio is null ? NotFound() : Ok(patio);
    }

    /// <summary>
    /// Cria um novo pátio.
    /// </summary>
    [HttpPost]
    public IActionResult Post([FromBody] Patio patio)
    {
        if (patio == null) return BadRequest("Dados inválidos");
        var novo = _service.Criar(patio);
        return CreatedAtAction(nameof(GetById), new { id = novo.PatioId }, novo);
    }

    /// <summary>
    /// Atualiza um pátio existente.
    /// </summary>
    [HttpPut("{id}")]
    public IActionResult Put(Guid id, [FromBody] Patio patio)
    {
        if (patio == null || id != patio.PatioId) return BadRequest();
        var atualizado = _service.Atualizar(id, patio);
        return atualizado ? Ok(patio) : NotFound();
    }

    /// <summary>
    /// Remove um pátio pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var removido = _service.Remover(id);
        return removido ? NoContent() : NotFound();
    }
}
