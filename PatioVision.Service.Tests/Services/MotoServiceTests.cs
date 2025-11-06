using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Enums;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Services;
using Xunit;

namespace PatioVision.Service.Tests.Services;

/// <summary>
/// Testes unitários para MotoService
/// Cobre operações CRUD, paginação, filtros e validações
/// </summary>
public class MotoServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly MotoService _service;

    public MotoServiceTests()
    {
        // Configurar banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PatioVision_Test_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _service = new MotoService(_context);
    }

    #region Create Tests

    [Fact]
    public void Create_ComDadosValidos_DeveCriarMotoComSucesso()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = new Moto
        {
            Modelo = "CG 160",
            Placa = "ABC1D23",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act
        var resultado = _service.Create(moto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.MotoId.Should().NotBeEmpty();
        resultado.Modelo.Should().Be("CG 160");
        resultado.Placa.Should().Be("ABC1D23");
        resultado.DtCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        resultado.DtAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verificar que foi salvo no banco
        var motoNoBanco = _context.Motos.Find(resultado.MotoId);
        motoNoBanco.Should().NotBeNull();
    }

    [Fact]
    public void Create_ComModeloVazio_DeveLancarArgumentException()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = new Moto
        {
            Modelo = "",
            Placa = "ABC1D23",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.Create(moto));
        exception.Message.Should().Contain("obrigatório");
    }

    [Fact]
    public void Create_ComMotoNull_DeveLancarArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.Create(null!));
    }

    #endregion

    #region GetById Tests

    [Fact]
    public void GetById_ComIdExistente_DeveRetornarMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        _context.Motos.Add(moto);
        _context.SaveChanges();

        // Act
        var resultado = _service.GetById(moto.MotoId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.MotoId.Should().Be(moto.MotoId);
        resultado.Modelo.Should().Be(moto.Modelo);
        resultado.Patio.Should().NotBeNull();
        resultado.Dispositivo.Should().NotBeNull();
    }

    [Fact]
    public void GetById_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = _service.GetById(idInexistente);

        // Assert
        resultado.Should().BeNull();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_ComDadosValidos_DeveAtualizarMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        _context.Motos.Add(moto);
        _context.SaveChanges();
        
        // Detach da entidade para evitar conflito de rastreamento ao fazer update
        _context.Entry(moto).State = EntityState.Detached;

        var motoAtualizada = new Moto
        {
            MotoId = moto.MotoId,
            Modelo = "PCX 150",
            Placa = "XYZ9Z99",
            Status = StatusMoto.EmManutencao,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act
        var resultado = _service.Update(moto.MotoId, motoAtualizada);

        // Assert
        resultado.Should().BeTrue();
        
        var motoNoBanco = _context.Motos.Find(moto.MotoId);
        motoNoBanco.Should().NotBeNull();
        motoNoBanco!.Modelo.Should().Be("PCX 150");
        motoNoBanco.Placa.Should().Be("XYZ9Z99");
        motoNoBanco.Status.Should().Be(StatusMoto.EmManutencao);
        motoNoBanco.DtAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Update_ComIdInexistente_DeveRetornarFalse()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var motoAtualizada = new Moto
        {
            MotoId = idInexistente,
            Modelo = "PCX 150",
            Placa = "XYZ9Z99",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act
        var resultado = _service.Update(idInexistente, motoAtualizada);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void Update_ComModeloVazio_DeveLancarArgumentException()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        _context.Motos.Add(moto);
        _context.SaveChanges();

        var motoAtualizada = new Moto
        {
            MotoId = moto.MotoId,
            Modelo = "",
            Placa = "XYZ9Z99",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.Update(moto.MotoId, motoAtualizada));
        exception.Message.Should().Contain("obrigatório");
    }

    [Fact]
    public void Update_ComIdDiferente_DeveLancarArgumentException()
    {
        // Arrange
        var motoAtualizada = new Moto
        {
            MotoId = Guid.NewGuid(),
            Modelo = "PCX 150",
            Status = StatusMoto.Disponivel,
            PatioId = Guid.NewGuid(),
            DispositivoIotId = Guid.NewGuid()
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.Update(Guid.NewGuid(), motoAtualizada));
        exception.Message.Should().Contain("inválidos");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ComIdExistente_DeveRemoverMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        _context.Motos.Add(moto);
        _context.SaveChanges();

        // Act
        var resultado = _service.Delete(moto.MotoId);

        // Assert
        resultado.Should().BeTrue();
        var motoNoBanco = _context.Motos.Find(moto.MotoId);
        motoNoBanco.Should().BeNull();
    }

    [Fact]
    public void Delete_ComIdInexistente_DeveRetornarFalse()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = _service.Delete(idInexistente);

        // Assert
        resultado.Should().BeFalse();
    }

    #endregion

    #region GetPagedAsync Tests

    [Fact]
    public async Task GetPagedAsync_ComPaginaValida_DeveRetornarMotosPaginadas()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        // Criar 15 motos para testar paginação
        for (int i = 1; i <= 15; i++)
        {
            var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
            moto.Modelo = $"Modelo {i}";
            _context.Motos.Add(moto);
        }
        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(pageNumber: 1, pageSize: 10);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Items.Should().HaveCount(10);
        resultado.TotalItems.Should().Be(15);
        resultado.TotalPages.Should().Be(2);
        resultado.PageNumber.Should().Be(1);
        resultado.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetPagedAsync_ComSegundaPagina_DeveRetornarMotosCorretas()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        for (int i = 1; i <= 15; i++)
        {
            var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
            moto.Modelo = $"Modelo {i}";
            _context.Motos.Add(moto);
        }
        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(pageNumber: 2, pageSize: 10);

        // Assert
        resultado.Items.Should().HaveCount(5);
        resultado.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_ComFiltroSearch_DeveFiltrarPorModelo()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto1 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto1.Modelo = "CG 160";
        _context.Motos.Add(moto1);

        var moto2 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto2.Modelo = "PCX 150";
        _context.Motos.Add(moto2);

        var moto3 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto3.Modelo = "CG 125";
        _context.Motos.Add(moto3);

        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(search: "CG");

        // Assert
        resultado.Items.Should().HaveCount(2);
        resultado.Items.All(m => m.Modelo.Contains("CG")).Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_ComFiltroSearch_DeveFiltrarPorPlaca()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto1 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto1.Placa = "ABC1D23";
        _context.Motos.Add(moto1);

        var moto2 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto2.Placa = "XYZ9Z99";
        _context.Motos.Add(moto2);

        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(search: "ABC");

        // Assert
        resultado.Items.Should().HaveCount(1);
        resultado.Items.First().Placa.Should().Be("ABC1D23");
    }

    [Fact]
    public async Task GetPagedAsync_ComOrdenacaoModelo_DeveOrdenarCorretamente()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto1 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto1.Modelo = "Zebra";
        _context.Motos.Add(moto1);

        var moto2 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto2.Modelo = "Alpha";
        _context.Motos.Add(moto2);

        var moto3 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto3.Modelo = "Beta";
        _context.Motos.Add(moto3);

        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(sort: "modelo");

        // Assert
        resultado.Items.Should().HaveCount(3);
        resultado.Items[0].Modelo.Should().Be("Alpha");
        resultado.Items[1].Modelo.Should().Be("Beta");
        resultado.Items[2].Modelo.Should().Be("Zebra");
    }

    [Fact]
    public async Task GetPagedAsync_ComOrdenacaoDescendente_DeveOrdenarCorretamente()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        _context.SaveChanges();

        var moto1 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto1.Modelo = "Alpha";
        _context.Motos.Add(moto1);

        var moto2 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto2.Modelo = "Beta";
        _context.Motos.Add(moto2);

        var moto3 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto3.Modelo = "Zebra";
        _context.Motos.Add(moto3);

        _context.SaveChanges();

        // Act
        var resultado = await _service.GetPagedAsync(sort: "-modelo");

        // Assert
        resultado.Items.Should().HaveCount(3);
        resultado.Items[0].Modelo.Should().Be("Zebra");
        resultado.Items[1].Modelo.Should().Be("Beta");
        resultado.Items[2].Modelo.Should().Be("Alpha");
    }

    #endregion

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

    private Patio CriarPatio()
    {
        return new Patio
        {
            PatioId = Guid.NewGuid(),
            Nome = "Pátio Teste",
            Categoria = PatioVision.Core.Enums.CategoriaPatio.Aluguel,
            Latitude = -23.5631m,
            Longitude = -46.6544m,
            Capacidade = 50
        };
    }

    private DispositivoIoT CriarDispositivo()
    {
        return new DispositivoIoT
        {
            DispositivoIotId = Guid.NewGuid(),
            Tipo = PatioVision.Core.Enums.TipoDispositivo.Moto,
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

