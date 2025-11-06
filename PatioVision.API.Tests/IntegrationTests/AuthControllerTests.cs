using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PatioVision.API.DTOs;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PatioVision.API.Tests.IntegrationTests;

/// <summary>
/// Testes de integração para AuthController
/// Testa o endpoint de login com fluxo completo
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<PatioVision.API.Program>>, IDisposable
{
    private readonly CustomWebApplicationFactory<PatioVision.API.Program> _factory;
    private readonly HttpClient _client;
    private readonly AppDbContext _context;

    public AuthControllerTests(CustomWebApplicationFactory<PatioVision.API.Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Criar scope para acessar o contexto
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DeveRetornarTokenJWT()
    {
        // Arrange
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

        var loginRequest = new LoginRequest
        {
            Email = "teste@mottu.com",
            Senha = "Senha123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Se falhar, verificar o conteúdo da resposta para diagnóstico
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Login falhou com status {response.StatusCode}: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.TokenType.Should().Be("Bearer");
        loginResponse.Email.Should().Be("teste@mottu.com");
        loginResponse.Nome.Should().Be("Teste Usuario");
        loginResponse.UsuarioId.Should().Be(usuario.Id);
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
    {
        // Arrange
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

        var loginRequest = new LoginRequest
        {
            Email = "teste@mottu.com",
            Senha = "SenhaErrada!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_DeveRetornarUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = "email_inexistente@mottu.com",
            Senha = "Senha123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_DeveRetornarUnauthorized()
    {
        // Arrange
        var senhaHash = BCrypt.Net.BCrypt.HashPassword("Senha123!");
        var usuario = new Usuario
        {
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = senhaHash,
            Perfil = Guid.NewGuid(),
            Ativo = false, // Usuário inativo
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        var loginRequest = new LoginRequest
        {
            Email = "teste@mottu.com",
            Senha = "Senha123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ComDadosNulos_DeveRetornarBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Email = null!,
            Senha = null!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    public void Dispose()
    {
        _client?.Dispose();
        
        // Limpar dados do teste - usar um novo contexto para evitar problemas de tracking
        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var usuarios = context.Usuario.ToList();
                if (usuarios.Any())
                {
                    context.Usuario.RemoveRange(usuarios);
                    context.SaveChanges();
                }
            }
        }
        catch
        {
            // Ignorar erros de limpeza - podem ocorrer se os dados já foram removidos
        }
    }
}

