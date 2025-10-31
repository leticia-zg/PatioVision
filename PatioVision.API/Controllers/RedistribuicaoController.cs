using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PatioVision.API.DTOs;
using PatioVision.Core.Models;
using PatioVision.Service.Services;

namespace PatioVision.API.Controllers;

/// <summary>
/// Endpoints para recomendações de redistribuição de motos usando ML.NET
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/redistribuicao")]
[Produces("application/json")]
[Authorize]
public class RedistribuicaoController : ControllerBase
{
    private readonly RedistribuicaoService _service;

    public RedistribuicaoController(RedistribuicaoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gera recomendações de redistribuição de motos entre pátios usando ML.NET.
    /// </summary>
    /// <remarks>
    /// Analisa todas as motos disponíveis (ou as especificadas) e recomenda redistribuições que:
    /// - Melhoram o equilíbrio entre pátios
    /// - Potencializam a taxa de atendimento
    /// - Reduzem congestionamento em pátios lotados
    /// - Otimizam a localização baseada em padrões históricos
    /// 
    /// **Exemplo de payload**:
    /// ```json
    /// {
    ///   "motoIds": ["guid1", "guid2"],
    ///   "patioIds": ["guid3", "guid4"]
    /// }
    /// ```
    /// Se `motoIds` for null/vazio, analisa todas as motos disponíveis.
    /// Se `patioIds` for null/vazio, considera todos os pátios como destino.
    /// </remarks>
    /// <param name="request">Filtros opcionais para motos e pátios específicos</param>
    /// <response code="200">Recomendações geradas com sucesso</response>
    /// <response code="400">Parâmetros inválidos</response>
    /// <response code="500">Erro ao processar recomendações</response>
    [HttpPost("recomendar")]
    [ProducesResponseType(typeof(RedistribuicaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Recomendar([FromBody] RedistribuicaoRequest? request = null)
    {
        try
        {
            request ??= new RedistribuicaoRequest();

            var (recomendacoes, metricasDict) = await _service.GerarRecomendacoesComMetricasAsync(
                request.MotoIds,
                request.PatioIds
            );

            // Enriquecer recomendações com dados das motos
            var recomendacoesDetalhadas = await EnriquecerRecomendacoesAsync(recomendacoes);

            var response = new RedistribuicaoResponse
            {
                Recomendacoes = recomendacoesDetalhadas,
                TotalRecomendacoes = recomendacoesDetalhadas.Count,
                Metricas = new MetricasDistribuicao
                {
                    TotalMotos = (int)metricasDict["TotalMotos"],
                    TotalPatios = (int)metricasDict["TotalPatios"],
                    MediaMotosPorPatio = (float)metricasDict["MediaMotosPorPatio"],
                    DesvioPadraoAtual = (float)metricasDict["DesvioPadraoAtual"],
                    DesvioPadraoEstimado = (float)metricasDict["DesvioPadraoEstimado"],
                    MelhoriaEquilibrioPercentual = (float)metricasDict["MelhoriaEquilibrioPercentual"],
                    PatiosCongestionados = (int)metricasDict["PatiosCongestionados"],
                    PatiosSubutilizados = (int)metricasDict["PatiosSubutilizados"]
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao gerar recomendações: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Erro ao processar recomendações", message = ex.Message }
            );
        }
    }

    /// <summary>
    /// Enriquece as recomendações com dados completos das motos
    /// </summary>
    private async Task<List<RecomendacaoDetalhada>> EnriquecerRecomendacoesAsync(
        List<PatioVision.Core.Models.ML.RedistribuicaoOutput> recomendacoes)
    {
        return recomendacoes.Select(rec => new RecomendacaoDetalhada
        {
            MotoId = rec.MotoId,
            MotoModelo = rec.MotoModelo,
            MotoPlaca = rec.MotoPlaca,
            PatioOrigemId = rec.PatioOrigemId,
            PatioOrigemNome = rec.PatioOrigemNome,
            Score = rec.Score,
            PatioDestinoId = rec.PatioDestinoId,
            PatioDestinoNome = rec.PatioDestinoNome,
            Motivos = rec.Motivos,
            ImpactoEquilibrio = rec.ImpactoEquilibrio,
            Recomendacao = rec.Recomendacao
        }).ToList();
    }
}

