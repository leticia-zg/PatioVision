using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PatioVision.Data.Seeders;

namespace PatioVision.API.Controllers;

/// <summary>
/// Endpoints para execução de seeders de dados
/// </summary>

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/seeder")]
[Produces("application/json")]
[AllowAnonymous]
public class SeederController : ControllerBase
{
    private readonly MlTrainingDataSeeder _seeder;

    public SeederController(MlTrainingDataSeeder seeder)
    {
        _seeder = seeder;
    }

    /// <summary>
    /// Executa o seeder de dados de treinamento ML.
    /// </summary>
    /// <remarks>
    /// Este endpoint popula o banco de dados com dados de treinamento para o modelo ML:
    /// - 270 dispositivos IoT (150 para motos + 120 para pátios)
    /// - 100 pátios com diferentes categorias e localizações
    /// - 140 motos distribuídas de forma desequilibrada entre os pátios
    /// - 100 usuários com perfis variados
    /// 
    /// **Importante**: Este endpoint verifica se já existem dados no banco. 
    /// Se houver dados, o seed será pulado para evitar duplicação.
    /// 
    /// **Recomendação**: Execute este endpoint antes de usar o endpoint de redistribuição 
    /// pela primeira vez para garantir que há dados suficientes para treinar o modelo ML.
    /// </remarks>
    /// <response code="200">Seeder executado com sucesso ou dados já existentes</response>
    /// <response code="500">Erro ao executar seeder</response>
    [HttpPost("ml-training-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ExecutarSeederMLTrainingData()
    {
        try
        {
            await _seeder.SeedAsync();

            return Ok(new
            {
                message = "Seeder executado com sucesso. Dados de treinamento ML foram populados no banco de dados.",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao executar seeder: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    error = "Erro ao executar seeder",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                }
            );
        }
    }
}

