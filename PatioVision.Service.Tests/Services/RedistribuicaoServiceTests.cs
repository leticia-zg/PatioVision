using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Enums;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Services;
using PatioVision.Service.Services.ML;
using Xunit;

namespace PatioVision.Service.Tests.Services;

/// <summary>
/// Testes unitários para RedistribuicaoService
/// Foca em cálculo de métricas e integração com ML Service
/// </summary>
public class RedistribuicaoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RedistribuicaoMLService _mlService;
    private readonly RedistribuicaoService _service;

    public RedistribuicaoServiceTests()
    {
        // Configurar banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PatioVision_Redistribuicao_Test_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _mlService = new RedistribuicaoMLService(_context);
        _service = new RedistribuicaoService(_context, _mlService);
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComDadosValidos_DeveRetornarMetricasCorretas()
    {
        // Arrange
        var patio1 = CriarPatio("Pátio A", 50);
        var patio2 = CriarPatio("Pátio B", 30);
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();

        _context.Patios.AddRange(patio1, patio2);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        await _context.SaveChangesAsync();

        // Criar 10 motos no pátio 1 e 5 no pátio 2
        for (int i = 0; i < 10; i++)
        {
            var moto = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
            moto.Modelo = $"Moto {i + 1}";
            _context.Motos.Add(moto);
        }

        for (int i = 0; i < 5; i++)
        {
            var moto = CriarMoto(patio2.PatioId, dispositivo2.DispositivoIotId);
            moto.Modelo = $"Moto {i + 11}";
            _context.Motos.Add(moto);
        }

        await _context.SaveChangesAsync();

        // Act
        var (recomendacoes, metricas) = await _service.GerarRecomendacoesComMetricasAsync();

        // Assert
        recomendacoes.Should().NotBeNull();
        metricas.Should().NotBeNull();

        // Verificar métricas básicas
        metricas["TotalMotos"].Should().Be(15);
        metricas["TotalPatios"].Should().Be(2);
        metricas["MediaMotosPorPatio"].Should().Be(7.5f);
        metricas["PatiosCongestionados"].Should().BeOfType<int>();
        metricas["PatiosSubutilizados"].Should().BeOfType<int>();
        metricas["DesvioPadraoAtual"].Should().BeOfType<float>();
        metricas["DesvioPadraoEstimado"].Should().BeOfType<float>();
        metricas["MelhoriaEquilibrioPercentual"].Should().BeOfType<float>();
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComPatiosVazios_DeveLancarExcecao()
    {
        // Arrange
        // Criar pelo menos 2 pátios mas sem motos
        var patio1 = CriarPatio("Pátio A", 50);
        var patio2 = CriarPatio("Pátio B", 50);
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();
        _context.Patios.AddRange(patio1, patio2);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        await _context.SaveChangesAsync();

        // Act & Assert
        // Quando não há motos, o ML não pode treinar e deve lançar exceção
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.GerarRecomendacoesComMetricasAsync());
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComFiltroMotoIds_DeveFiltrarCorretamente()
    {
        // Arrange
        // Criar pelo menos 2 pátios para permitir treinamento do modelo
        var patio1 = CriarPatio("Pátio A", 50);
        var patio2 = CriarPatio("Pátio B", 50);
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();
        _context.Patios.AddRange(patio1, patio2);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        await _context.SaveChangesAsync();

        var moto1 = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
        var moto2 = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
        _context.Motos.AddRange(moto1, moto2);
        await _context.SaveChangesAsync();

        var motoIds = new List<Guid> { moto1.MotoId };

        // Act
        var (recomendacoes, metricas) = await _service.GerarRecomendacoesComMetricasAsync(motoIds);

        // Assert
        // Verificar que apenas a moto1 foi considerada nas métricas
        metricas["TotalMotos"].Should().Be(1);
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComFiltroPatioIds_DeveFiltrarCorretamente()
    {
        // Arrange
        // Criar pelo menos 2 pátios e algumas motos para permitir treinamento do modelo
        var patio1 = CriarPatio("Pátio A", 50);
        var patio2 = CriarPatio("Pátio B", 30);
        var patio3 = CriarPatio("Pátio C", 40);
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();
        _context.Patios.AddRange(patio1, patio2, patio3);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        
        // Adicionar algumas motos para permitir treinamento
        for (int i = 0; i < 3; i++)
        {
            var moto = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
            _context.Motos.Add(moto);
        }
        for (int i = 0; i < 2; i++)
        {
            var moto = CriarMoto(patio2.PatioId, dispositivo2.DispositivoIotId);
            _context.Motos.Add(moto);
        }
        
        await _context.SaveChangesAsync();

        var patioIds = new List<Guid> { patio1.PatioId, patio2.PatioId };

        // Act
        var (recomendacoes, metricas) = await _service.GerarRecomendacoesComMetricasAsync(null, patioIds);

        // Assert
        // Verificar que métricas consideram apenas pátios filtrados
        metricas["TotalPatios"].Should().Be(2);
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComPatiosCongestionados_DeveIdentificarCorretamente()
    {
        // Arrange
        // Usar capacidade maior para ter mais flexibilidade no cálculo
        var patio1 = CriarPatio("Pátio A", 20); // Capacidade pequena
        var patio2 = CriarPatio("Pátio B", 50); // Capacidade grande
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();

        _context.Patios.AddRange(patio1, patio2);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        await _context.SaveChangesAsync();

        // Criar 19 motos no pátio 1 (95% ocupação - congestionado > 90%)
        for (int i = 0; i < 19; i++)
        {
            var moto = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
            _context.Motos.Add(moto);
        }

        // Criar 3 motos no pátio 2 (6% ocupação - subutilizado < 50%)
        for (int i = 0; i < 3; i++)
        {
            var moto = CriarMoto(patio2.PatioId, dispositivo2.DispositivoIotId);
            _context.Motos.Add(moto);
        }

        await _context.SaveChangesAsync();

        // Act
        var (recomendacoes, metricas) = await _service.GerarRecomendacoesComMetricasAsync();

        // Assert
        // 19/20 = 0.95 = 95% > 90% (congestionado)
        ((int)metricas["PatiosCongestionados"]).Should().BeGreaterOrEqualTo(1);
        // 3/50 = 0.06 = 6% < 50% (subutilizado)
        ((int)metricas["PatiosSubutilizados"]).Should().BeGreaterOrEqualTo(1);
    }

    [Fact]
    public async Task GerarRecomendacoesComMetricasAsync_ComMotosNaoDisponiveis_DeveIgnorar()
    {
        // Arrange
        // Criar pelo menos 2 pátios para permitir treinamento do modelo
        var patio1 = CriarPatio("Pátio A", 50);
        var patio2 = CriarPatio("Pátio B", 50);
        var dispositivo1 = CriarDispositivo();
        var dispositivo2 = CriarDispositivo();
        _context.Patios.AddRange(patio1, patio2);
        _context.Dispositivos.AddRange(dispositivo1, dispositivo2);
        await _context.SaveChangesAsync();

        var motoDisponivel = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
        motoDisponivel.Status = StatusMoto.Disponivel;

        var motoAlugada = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
        motoAlugada.Status = StatusMoto.Alugada;

        var motoManutencao = CriarMoto(patio1.PatioId, dispositivo1.DispositivoIotId);
        motoManutencao.Status = StatusMoto.EmManutencao;
        
        // Adicionar mais uma moto disponível para permitir treinamento
        var motoDisponivel2 = CriarMoto(patio2.PatioId, dispositivo2.DispositivoIotId);
        motoDisponivel2.Status = StatusMoto.Disponivel;

        _context.Motos.AddRange(motoDisponivel, motoAlugada, motoManutencao, motoDisponivel2);
        await _context.SaveChangesAsync();

        // Act
        var (recomendacoes, metricas) = await _service.GerarRecomendacoesComMetricasAsync();

        // Assert
        // Apenas motos disponíveis devem ser consideradas
        metricas["TotalMotos"].Should().Be(2);
    }

    #region Helper Methods

    private Moto CriarMoto(Guid patioId, Guid dispositivoId)
    {
        return new Moto
        {
            Modelo = "CG 160",
            Placa = "ABC1D23",
            Status = StatusMoto.Disponivel,
            PatioId = patioId,
            DispositivoIotId = dispositivoId,
            DtCadastro = DateTime.UtcNow
        };
    }

    private Patio CriarPatio(string nome, int capacidade)
    {
        return new Patio
        {
            PatioId = Guid.NewGuid(),
            Nome = nome,
            Categoria = CategoriaPatio.Aluguel,
            Latitude = -23.5631m,
            Longitude = -46.6544m,
            Capacidade = capacidade,
            DispositivoIotId = Guid.NewGuid()
        };
    }

    private DispositivoIoT CriarDispositivo()
    {
        return new DispositivoIoT
        {
            DispositivoIotId = Guid.NewGuid(),
            Tipo = TipoDispositivo.Moto,
            UltimaLocalizacao = "Localização Teste",
            UltimaAtualizacao = DateTime.UtcNow
        };
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
    }
}

