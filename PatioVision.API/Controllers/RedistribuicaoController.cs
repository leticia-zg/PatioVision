using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
<<<<<<< Updated upstream
using PatioVision.API.DTOs;
using PatioVision.Core.Models;
=======
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
using PatioVision.API.DTOs;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
=======
using PatioVision.API.DTOs;
using PatioVision.Core.Models;
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
    private readonly AppDbContext _context;

    public RedistribuicaoController(RedistribuicaoService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }

    /// <summary>
    /// Gera recomendações de redistribuição de motos entre pátios usando ML.NET baseado na ocupação dos pátios.
    /// </summary>
    /// <remarks>
    /// Analisa as motos especificadas e recomenda redistribuições que:
    /// - Melhoram o equilíbrio entre pátios baseado na ocupação percentual
    /// - Reduzem congestionamento em pátios lotados
    /// - Otimizam a distribuição considerando a capacidade dos pátios
=======
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
    /// 
    /// **Exemplo de payload**:
    /// ```json
    /// {
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
    ///   "motoIds": ["guid1", "guid2"]
    /// }
    /// ```
    /// 
    /// **Resposta quando não há recomendações para uma moto específica**:
    /// Para cada moto solicitada sem recomendações, será retornada uma entrada com o campo `Mensagem` 
    /// contendo "nao há recomendações para essa moto".
    /// </remarks>
    /// <param name="request">Request contendo IDs das motos para análise</param>
=======
>>>>>>> Stashed changes
    ///   "motoIds": ["guid1", "guid2"],
    ///   "patioIds": ["guid3", "guid4"]
    /// }
    /// ```
    /// Se `motoIds` for null/vazio, analisa todas as motos disponíveis.
    /// Se `patioIds` for null/vazio, considera todos os pátios como destino.
    /// </remarks>
    /// <param name="request">Filtros opcionais para motos e pátios específicos</param>
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
=======
<<<<<<< HEAD
            Console.WriteLine($"[RedistribuicaoController] Iniciando geração de recomendações. MotoIds: {(request.MotoIds?.Count ?? 0)} motos");

            // Validar se foram fornecidos IDs de motos
            if (request.MotoIds == null || !request.MotoIds.Any())
            {
                return BadRequest(new { error = "É necessário fornecer pelo menos um ID de moto no campo 'motoIds'" });
            }

            // Verificar se as motos existem e estão disponíveis
            var motosSolicitadas = await _context.Motos
                .Where(m => request.MotoIds.Contains(m.MotoId))
                .Select(m => new { m.MotoId, m.Modelo, m.Placa, m.Status })
                .AsNoTracking()
                .ToListAsync();

            if (!motosSolicitadas.Any())
            {
                return BadRequest(new { error = "Nenhuma das motos informadas foi encontrada no sistema" });
            }

            Console.WriteLine($"[RedistribuicaoController] {motosSolicitadas.Count} motos encontradas para processamento");

            // Gerar recomendações baseadas na ocupação dos pátios
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
            var (recomendacoes, metricasDict) = await _service.GerarRecomendacoesComMetricasAsync(
                request.MotoIds,
                request.PatioIds
            );

<<<<<<< Updated upstream
            // Enriquecer recomendações com dados das motos
            var recomendacoesDetalhadas = await EnriquecerRecomendacoesAsync(recomendacoes);
=======
<<<<<<< HEAD
            Console.WriteLine($"[RedistribuicaoController] {recomendacoes.Count} recomendações geradas pelo serviço");

            // Agrupar recomendações por moto
            var recomendacoesPorMoto = recomendacoes
                .GroupBy(r => r.MotoId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Criar lista de recomendações detalhadas, garantindo uma entrada para cada moto solicitada
            var recomendacoesDetalhadas = new List<RecomendacaoDetalhada>();

            foreach (var moto in motosSolicitadas)
            {
                // Se a moto não está disponível, adicionar mensagem de erro
                if (moto.Status != Core.Enums.StatusMoto.Disponivel)
                {
                    recomendacoesDetalhadas.Add(new RecomendacaoDetalhada
                    {
                        MotoId = moto.MotoId,
                        MotoModelo = moto.Modelo ?? "Modelo não informado",
                        MotoPlaca = moto.Placa,
                        Mensagem = "Esta moto não está disponível para redistribuição. Status atual: " + moto.Status.ToString()
                    });
                    continue;
                }

                // Se há recomendações para esta moto, adicionar todas (ou apenas a melhor)
                if (recomendacoesPorMoto.TryGetValue(moto.MotoId, out var recomsDaMoto) && recomsDaMoto.Any())
                {
                    // Pegar apenas a melhor recomendação para cada moto (maior score)
                    var melhorRecomendacao = recomsDaMoto
                        .OrderByDescending(r => r.Score)
                        .ThenByDescending(r => r.ImpactoEquilibrio)
                        .First();

                    recomendacoesDetalhadas.Add(new RecomendacaoDetalhada
                    {
                        MotoId = melhorRecomendacao.MotoId,
                        MotoModelo = melhorRecomendacao.MotoModelo,
                        MotoPlaca = melhorRecomendacao.MotoPlaca,
                        PatioOrigemId = melhorRecomendacao.PatioOrigemId,
                        PatioOrigemNome = melhorRecomendacao.PatioOrigemNome,
                        Score = melhorRecomendacao.Score,
                        PatioDestinoId = melhorRecomendacao.PatioDestinoId,
                        PatioDestinoNome = melhorRecomendacao.PatioDestinoNome,
                        Motivos = melhorRecomendacao.Motivos,
                        ImpactoEquilibrio = melhorRecomendacao.ImpactoEquilibrio,
                        Recomendacao = melhorRecomendacao.Recomendacao
                    });
                }
                else
                {
                    // Não há recomendações para esta moto específica
                    recomendacoesDetalhadas.Add(new RecomendacaoDetalhada
                    {
                        MotoId = moto.MotoId,
                        MotoModelo = moto.Modelo ?? "Modelo não informado",
                        MotoPlaca = moto.Placa,
                        Mensagem = "nao há recomendações para essa moto"
                    });
                }
            }

            Console.WriteLine($"[RedistribuicaoController] Processamento concluído. {recomendacoesDetalhadas.Count} entradas geradas");

            // Determinar mensagem geral se não houver nenhuma recomendação
            string? mensagemGeral = null;
            var totalRecomendacoes = recomendacoesDetalhadas.Count(r => !string.IsNullOrEmpty(r.Recomendacao));
            
            if (totalRecomendacoes == 0)
            {
                mensagemGeral = "Nenhuma recomendação foi gerada para as motos solicitadas. Verifique se existem pátios com capacidade disponível.";
            }
=======
            // Enriquecer recomendações com dados das motos
            var recomendacoesDetalhadas = await EnriquecerRecomendacoesAsync(recomendacoes);
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes

            var response = new RedistribuicaoResponse
            {
                Recomendacoes = recomendacoesDetalhadas,
<<<<<<< Updated upstream
                TotalRecomendacoes = recomendacoesDetalhadas.Count,
=======
<<<<<<< HEAD
                TotalRecomendacoes = totalRecomendacoes,
                Mensagem = mensagemGeral,
=======
                TotalRecomendacoes = recomendacoesDetalhadas.Count,
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            Console.WriteLine($"Erro ao gerar recomendações: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
=======
<<<<<<< HEAD
            Console.WriteLine($"[RedistribuicaoController] Erro ao gerar recomendações: {ex.Message}");
            Console.WriteLine($"[RedistribuicaoController] Stack trace: {ex.StackTrace}");
=======
            Console.WriteLine($"Erro ao gerar recomendações: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Erro ao processar recomendações", message = ex.Message }
            );
        }
    }
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
=======
>>>>>>> Stashed changes

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
<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
}

