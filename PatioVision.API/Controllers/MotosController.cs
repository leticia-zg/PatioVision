using Microsoft.AspNetCore.Mvc;
using PatioVision.Core.Models;
using PatioVision.Service.Services;
using PatioVision.Core.Enums;
using System;

namespace PatioVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoService _service;

    public MotosController(MotoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna todas as motos cadastradas.
    /// </summary>
    /// <remarks>Lista todas as motos, com os dados do pátio e do dispositivo IoT.</remarks>
    /// <response code="200">Lista de motos retornada com sucesso.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var motos = _service.ObterTodas();
        return Ok(motos);
    }

    /// <summary>
    /// Retorna uma moto específica pelo ID.
    /// </summary>
    /// <param name="id">ID da moto.</param>
    /// <response code="200">Moto encontrada com sucesso.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(Guid id)
    {
        var moto = _service.ObterPorId(id);
        if (moto == null)
            return NotFound();

        return Ok(moto);
    }

    /// <summary>
    /// Cria uma nova moto.
    /// </summary>
    /// <param name="moto">Dados da moto a ser cadastrada.</param>
    /// <response code="201">Moto criada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] Moto moto)
    {
        if (moto == null || string.IsNullOrEmpty(moto.Modelo))
            return BadRequest("Dados inválidos para criação.");

        var novaMoto = _service.Criar(moto);
        return CreatedAtAction(nameof(GetById), new { id = novaMoto.MotoId }, novaMoto);
    }

    /// <summary>
    /// Atualiza os dados de uma moto existente.
    /// </summary>
    /// <param name="id">ID da moto.</param>
    /// <param name="moto">Dados atualizados da moto.</param>
    /// <response code="200">Moto atualizada com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(Guid id, [FromBody] Moto moto)
    {
        if (moto == null || id == Guid.Empty || id != moto.MotoId)
            return BadRequest("Dados inválidos para atualização.");

        var atualizada = _service.Atualizar(id, moto);
        if (!atualizada)
            return NotFound();

        return Ok(moto);
    }

    /// <summary>
    /// Remove uma moto pelo ID.
    /// </summary>
    /// <param name="id">ID da moto a ser removida.</param>
    /// <response code="204">Moto removida com sucesso.</response>
    /// <response code="400">ID inválido.</response>
    /// <response code="404">Moto não encontrada.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Id inválido.");

        var removida = _service.Remover(id);
        if (!removida)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Retorna motos filtradas por status.
    /// </summary>
    /// <param name="valor">Status da moto (ex: Disponivel, Alugada, EmManutencao).</param>
    /// <response code="200">Motos filtradas por status.</response>
    /// <response code="400">Status inválido.</response>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GetByStatus([FromQuery] string valor)
    {
        if (!Enum.TryParse<StatusMoto>(valor, true, out var status))
            return BadRequest("Status inválido. Use: Disponivel, EmManutencao ou Alugada.");

        var motos = _service.ObterPorStatus(status);
        return Ok(motos);
    }
}
