using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PatioVision.API.DTOs;
using PatioVision.Core.Enums;
using PatioVision.Service.Services.ML;

namespace PatioVision.API.Controllers;

/// <summary>
/// Endpoints para otimização inteligente de distribuição de motos entre pátios usando ML.NET.
/// </summary>
/// <remarks>
/// Este controller utiliza Machine Learning para recomendar a melhor redistribuição de motos,
/// considerando capacidade dos pátios, demanda histórica, localização geográfica e padrões temporais.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/patios")]
[Produces("application/json")]
[Authorize]
public class OtimizacaoDistribuicaoController : ControllerBase
{
    private readonly OtimizacaoDistribuicaoService _mlService;

    public OtimizacaoDistribuicaoController(OtimizacaoDistribuicaoService mlService)
    {
        _mlService = mlService;
    }

    /// <summary>
    /// Otimiza a distribuição de motos entre pátios usando Machine Learning.
    /// </summary>
    /// <remarks>
    /// Analisa todas as motos disponíveis (ou as especificadas) e recomenda redistribuições
    /// que melhoram o equilíbrio entre pátios e potencializam a taxa de atendimento.
    /// 
    /// **Exemplo de request**:
    /// ```json
    /// {
    ///   "motoIds": null,
    ///   "apenasDisponiveis": true,
    ///   "incluirTodasMotos": false
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">Parâmetros da otimização.</param>
    /// <response code="200">Recomendações geradas com sucesso (mesmo que não haja recomendações).</response>
    /// <response code="400">Parâmetros inválidos ou erro na execução.</response>
    [HttpPost("otimizar-distribuicao")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OtimizarDistribuicao([FromBody] OtimizacaoDistribuicaoRequest? request = null)
    {
        request ??= new OtimizacaoDistribuicaoRequest();

        try
        {
            // Valida e filtra GUIDs inválidos ou vazios
            List<Guid>? motoIdsValidos = null;
            if (request.MotoIds != null && request.MotoIds.Any())
            {
                motoIdsValidos = request.MotoIds
                    .Where(id => id != Guid.Empty)
                    .ToList();
                
                // Se após filtrar ficou vazio, trata como null
                if (!motoIdsValidos.Any())
                {
                    motoIdsValidos = null;
                }
            }

            var (recomendacoes, totalAnalisadas) = await _mlService.GerarRecomendacoesAsync(
                motoIdsValidos,
                request.ApenasDisponiveis,
                request.IncluirTodasMotos);

            var response = new OtimizacaoDistribuicaoResponse
            {
                TotalMotosAnalisadas = totalAnalisadas,
                TotalRecomendacoes = recomendacoes.Count,
                Recomendacoes = recomendacoes.Select(r => new RecomendacaoDistribuicao
                {
                    MotoId = r.moto.MotoId,
                    ModeloMoto = r.moto.Modelo,
                    StatusAtual = r.moto.Status.ToString(),
                    PatioAtualId = r.patioAtual.PatioId,
                    PatioAtualNome = r.patioAtual.Nome,
                    PatioRecomendadoId = r.patioRecomendado.PatioId,
                    PatioRecomendadoNome = r.patioRecomendado.Nome,
                    ScoreAdequacao = r.score,
                    MelhoriaEsperada = (r.score - 0.5f) * 100f, // Melhoria percentual estimada
                    Motivo = GerarMotivo(r.moto, r.patioAtual, r.patioRecomendado, r.score),
                    Beneficios = GerarBeneficios(r.moto, r.patioAtual, r.patioRecomendado, r.score)
                }).ToList(),
                Estatisticas = CalcularEstatisticas(recomendacoes, totalAnalisadas)
            };

            return Ok(new
            {
                data = response,
                _links = new Dictionary<string, object>
                {
                    ["self"] = new { href = $"{Request.Scheme}://{Request.Host}{Request.Path}", method = "POST" }
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gera um motivo textual para a recomendação.
    /// </summary>
    private string GerarMotivo(Core.Models.Moto moto, Core.Models.Patio patioAtual, Core.Models.Patio patioRecomendado, float score)
    {
        var motivos = new List<string>();

        // Análise de categoria
        if (moto.Status == StatusMoto.Disponivel && patioRecomendado.Categoria == CategoriaPatio.Aluguel)
        {
            motivos.Add("Pátio especializado em aluguel possui maior demanda");
        }
        else if (moto.Status == StatusMoto.EmManutencao && patioRecomendado.Categoria == CategoriaPatio.Manutencao)
        {
            motivos.Add("Pátio especializado em manutenção oferece melhor infraestrutura");
        }

        // Análise de ocupação
        var ocupacaoAtual = patioAtual.Motos.Count;
        var ocupacaoRecomendada = patioRecomendado.Motos.Count;
        if (ocupacaoAtual > ocupacaoRecomendada + 5)
        {
            motivos.Add("Reduz congestionamento no pátio atual");
        }

        if (patioRecomendado.Motos.Count < patioAtual.Motos.Count)
        {
            motivos.Add("Melhor balanceamento de capacidade entre pátios");
        }

        // Score alto
        if (score > 0.75f)
        {
            motivos.Add("Alta adequação prevista para este pátio");
        }

        return motivos.Any() 
            ? string.Join("; ", motivos)
            : "Otimização baseada em análise de ML dos padrões históricos";
    }

    /// <summary>
    /// Gera lista de benefícios esperados.
    /// </summary>
    private List<string> GerarBeneficios(Core.Models.Moto moto, Core.Models.Patio patioAtual, Core.Models.Patio patioRecomendado, float score)
    {
        var beneficios = new List<string>();

        if (score > 0.7f)
        {
            beneficios.Add("Redução estimada de 15-20% no tempo de atendimento");
        }

        if (patioRecomendado.Motos.Count < patioAtual.Motos.Count)
        {
            beneficios.Add("Melhor distribuição da capacidade entre pátios");
        }

        if (moto.Status == StatusMoto.Disponivel && patioRecomendado.Categoria == CategoriaPatio.Aluguel)
        {
            beneficios.Add("Aumento da probabilidade de aluguel pela localização estratégica");
        }

        beneficios.Add($"Score de adequação: {(score * 100):F1}%");

        return beneficios;
    }

    /// <summary>
    /// Calcula estatísticas agregadas das recomendações.
    /// </summary>
    private EstatisticasOtimizacao CalcularEstatisticas(
        List<(Core.Models.Moto moto, Core.Models.Patio patioAtual, Core.Models.Patio patioRecomendado, float score)> recomendacoes,
        int totalAnalisadas)
    {
        if (!recomendacoes.Any())
        {
            string mensagem;
            if (totalAnalisadas == 0)
            {
                mensagem = "Nenhuma moto encontrada com os critérios especificados";
            }
            else if (totalAnalisadas == 1)
            {
                mensagem = "Moto analisada, mas nenhuma redistribuição melhor foi encontrada. Pode estar já bem posicionada ou não há outros pátios disponíveis.";
            }
            else
            {
                mensagem = $"{totalAnalisadas} moto(s) analisada(s), mas nenhuma redistribuição melhor foi encontrada. Motos podem estar já bem posicionadas ou não há outros pátios disponíveis.";
            }

            return new EstatisticasOtimizacao
            {
                BeneficioEsperado = mensagem,
                ReducaoTempoMedioAtendimento = 0f,
                MelhoriaBalanceamentoPatio = 0f,
                PatiosAfetados = 0
            };
        }

        var scoreMedio = recomendacoes.Average(r => r.score);
        var patiosUnicos = recomendacoes
            .SelectMany(r => new[] { r.patioAtual.PatioId, r.patioRecomendado.PatioId })
            .Distinct()
            .Count();

        var reducaoTempoEstimada = Math.Max(10f, scoreMedio * 25f);
        var melhoriaBalanceamento = Math.Min(100f, scoreMedio * 150f);

        return new EstatisticasOtimizacao
        {
            BeneficioEsperado = $"Otimização de {recomendacoes.Count} moto(s) pode reduzir tempo de atendimento em até {reducaoTempoEstimada:F0}%",
            ReducaoTempoMedioAtendimento = reducaoTempoEstimada,
            MelhoriaBalanceamentoPatio = melhoriaBalanceamento,
            PatiosAfetados = patiosUnicos
        };
    }
}

