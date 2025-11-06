using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatioVision.Core.Enums;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PatioVision.API.Tests.IntegrationTests;

/// <summary>
/// Testes de integração para MotosController
/// Testa endpoints CRUD com autenticação
/// </summary>
public class MotosControllerTests : IClassFixture<CustomWebApplicationFactory<PatioVision.API.Program>>, IDisposable
{
    private readonly CustomWebApplicationFactory<PatioVision.API.Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _context;
    private readonly string _tokenJwt;

    public MotosControllerTests(CustomWebApplicationFactory<PatioVision.API.Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Criar scope para acessar o contexto
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Criar usuário e obter token para autenticação
        _tokenJwt = CriarUsuarioEToken().Result;
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenJwt);
    }

    [Fact]
    public async Task Get_ComAutenticacao_DeveRetornarListaPaginada()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
            moto.Modelo = $"Moto {i + 1}";
            _context.Motos.Add(moto);
        }
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/motos?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(5);
        json.RootElement.GetProperty("totalItems").GetInt32().Should().Be(5);
        json.RootElement.GetProperty("pageNumber").GetInt32().Should().Be(1);
        json.RootElement.GetProperty("pageSize").GetInt32().Should().Be(10);
    }

    [Fact]
    public async Task Get_SemAutenticacao_DeveRetornarUnauthorized()
    {
        // Arrange
        var clientSemAuth = _factory.CreateClient();

        // Act
        var response = await clientSemAuth.GetAsync("/api/v1/motos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_ComIdExistente_DeveRetornarMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto.Modelo = "CG 160";
        _context.Motos.Add(moto);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/motos/{moto.MotoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("data").GetProperty("motoId").GetString().Should().Be(moto.MotoId.ToString());
        json.RootElement.GetProperty("data").GetProperty("modelo").GetString().Should().Be("CG 160");
    }

    [Fact]
    public async Task GetById_ComIdInexistente_DeveRetornarNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/motos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Post_ComDadosValidos_DeveCriarMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var moto = new Moto
        {
            Modelo = "PCX 150",
            Placa = "XYZ9Z99",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/motos", moto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("data").GetProperty("modelo").GetString().Should().Be("PCX 150");
        json.RootElement.GetProperty("data").GetProperty("placa").GetString().Should().Be("XYZ9Z99");

        // Verificar que foi salvo no banco
        var motoNoBanco = await _context.Motos.FirstOrDefaultAsync(m => m.Modelo == "PCX 150");
        motoNoBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task Post_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var moto = new Moto
        {
            Modelo = "", // Modelo vazio
            Placa = "XYZ9Z99",
            Status = StatusMoto.Disponivel,
            PatioId = Guid.NewGuid(),
            DispositivoIotId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/motos", moto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Put_ComDadosValidos_DeveAtualizarMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto.Modelo = "CG 160";
        _context.Motos.Add(moto);
        await _context.SaveChangesAsync();

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
        var response = await _client.PutAsJsonAsync($"/api/v1/motos/{moto.MotoId}", motoAtualizada);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que foi atualizado no banco usando um novo contexto
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var motoNoBanco = await context.Motos.FindAsync(moto.MotoId);
            motoNoBanco.Should().NotBeNull();
            motoNoBanco!.Modelo.Should().Be("PCX 150");
        }
    }

    [Fact]
    public async Task Put_ComIdInexistente_DeveRetornarNotFound()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var motoAtualizada = new Moto
        {
            MotoId = Guid.NewGuid(),
            Modelo = "PCX 150",
            Placa = "XYZ9Z99",
            Status = StatusMoto.Disponivel,
            PatioId = patio.PatioId,
            DispositivoIotId = dispositivo.DispositivoIotId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/motos/{motoAtualizada.MotoId}", motoAtualizada);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ComIdExistente_DeveRemoverMoto()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var moto = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        _context.Motos.Add(moto);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/motos/{moto.MotoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que foi removido do banco usando um novo contexto
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var motoNoBanco = await context.Motos.FindAsync(moto.MotoId);
            motoNoBanco.Should().BeNull();
        }
    }

    [Fact]
    public async Task Delete_ComIdInexistente_DeveRetornarNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/v1/motos/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ComFiltroSearch_DeveFiltrarCorretamente()
    {
        // Arrange
        var patio = CriarPatio();
        var dispositivo = CriarDispositivo();
        _context.Patios.Add(patio);
        _context.Dispositivos.Add(dispositivo);
        await _context.SaveChangesAsync();

        var moto1 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto1.Modelo = "CG 160";
        moto1.Placa = "ABC1D23";
        _context.Motos.Add(moto1);

        var moto2 = CriarMoto(patio.PatioId, dispositivo.DispositivoIotId);
        moto2.Modelo = "PCX 150";
        moto2.Placa = "XYZ9Z99";
        _context.Motos.Add(moto2);

        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/v1/motos?search=CG");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        json.RootElement.GetProperty("items").GetArrayLength().Should().Be(1);
        json.RootElement.GetProperty("items")[0].GetProperty("data").GetProperty("modelo").GetString().Should().Be("CG 160");
    }

    #region Helper Methods

    private async Task<string> CriarUsuarioEToken()
    {
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("Senha123!");
        var usuario = new Usuario
        {
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = senhaHash,
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        // Fazer login para obter token
        var loginRequest = new PatioVision.API.DTOs.LoginRequest
        {
            Email = "teste@mottu.com",
            Senha = "Senha123!"
        };

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        
        // Verificar se o login foi bem-sucedido
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login falhou com status {response.StatusCode}: {errorContent}");
        }
        
        response.EnsureSuccessStatusCode();
        var loginResponse = await response.Content.ReadFromJsonAsync<PatioVision.API.DTOs.LoginResponse>();

        if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
        {
            throw new Exception("Token não foi retornado na resposta de login");
        }

        return loginResponse.Token;
    }

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
            Categoria = CategoriaPatio.Aluguel,
            Latitude = -23.5631m,
            Longitude = -46.6544m,
            Capacidade = 50,
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
        _client?.Dispose();
        
        // Limpar dados do teste - usar um novo contexto para evitar problemas de tracking
        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var motos = context.Motos.ToList();
                if (motos.Any())
                {
                    context.Motos.RemoveRange(motos);
                }

                var patios = context.Patios.ToList();
                if (patios.Any())
                {
                    context.Patios.RemoveRange(patios);
                }

                var dispositivos = context.Dispositivos.ToList();
                if (dispositivos.Any())
                {
                    context.Dispositivos.RemoveRange(dispositivos);
                }

                var usuarios = context.Usuario.ToList();
                if (usuarios.Any())
                {
                    context.Usuario.RemoveRange(usuarios);
                }

                context.SaveChanges();
            }
        }
        catch
        {
            // Ignorar erros de limpeza - podem ocorrer se os dados já foram removidos
        }
    }
}

