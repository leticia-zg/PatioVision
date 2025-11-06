using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PatioVision.Core.Models;
using PatioVision.Data.Context;
using PatioVision.Service.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace PatioVision.Service.Tests.Services;

/// <summary>
/// Testes unitários para AuthService
/// Cobre autenticação, geração de JWT e validação de email
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // Configurar banco em memória para testes
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"PatioVision_Auth_Test_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);

        // Configurar configurações JWT para teste
        var configurationDict = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "MinhaChaveSecretaSuperSeguraParaTestesComPeloMenos32Caracteres",
            ["Jwt:Issuer"] = "PatioVision.Test",
            ["Jwt:Audience"] = "PatioVision.Test",
            ["Jwt:ExpirationInMinutes"] = "480"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationDict)
            .Build();

        _service = new AuthService(_context, _configuration);
    }

    #region AuthenticateAsync Tests

    [Fact]
    public async Task AuthenticateAsync_ComCredenciaisValidas_DeveRetornarUsuario()
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

        // Act
        var resultado = await _service.AuthenticateAsync("teste@mottu.com", "Senha123!");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Email.Should().Be("teste@mottu.com");
        resultado.Nome.Should().Be("Teste Usuario");
    }

    [Fact]
    public async Task AuthenticateAsync_ComEmailInvalido_DeveRetornarNull()
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

        // Act
        var resultado = await _service.AuthenticateAsync("email_inexistente@mottu.com", "Senha123!");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ComSenhaInvalida_DeveRetornarNull()
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

        // Act
        var resultado = await _service.AuthenticateAsync("teste@mottu.com", "SenhaErrada!");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ComUsuarioInativo_DeveRetornarNull()
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

        // Act
        var resultado = await _service.AuthenticateAsync("teste@mottu.com", "Senha123!");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ComEmailVazio_DeveRetornarNull()
    {
        // Act
        var resultado = await _service.AuthenticateAsync("", "Senha123!");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ComSenhaVazia_DeveRetornarNull()
    {
        // Act
        var resultado = await _service.AuthenticateAsync("teste@mottu.com", "");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AuthenticateAsync_ComEmailCaseInsensitive_DeveFuncionar()
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

        // Act
        var resultado = await _service.AuthenticateAsync("TESTE@MOTTU.COM", "Senha123!");

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Email.Should().Be("teste@mottu.com");
    }

    #endregion

    #region GenerateJwtToken Tests

    [Fact]
    public void GenerateJwtToken_ComUsuarioValido_DeveGerarTokenJWT()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = "hash_fake",
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        // Act
        var token = _service.GenerateJwtToken(usuario);

        // Assert
        token.Should().NotBeNullOrEmpty();

        // Validar estrutura do token
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.Should().NotBeNull();
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == usuario.Id.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == usuario.Nome);
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == usuario.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == "PerfilId" && c.Value == usuario.Perfil.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jsonToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void GenerateJwtToken_ComUsuarioValido_DeveTerExpirationCorreta()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = "hash_fake",
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        // Act
        var token = _service.GenerateJwtToken(usuario);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(480), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GenerateJwtToken_ComUsuarioValido_DeveTerIssuerCorreto()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = "hash_fake",
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        // Act
        var token = _service.GenerateJwtToken(usuario);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be("PatioVision.Test");
        jsonToken.Audiences.Should().Contain("PatioVision.Test");
    }

    #endregion

    #region EmailExistsAsync Tests

    [Fact]
    public async Task EmailExistsAsync_ComEmailExistente_DeveRetornarTrue()
    {
        // Arrange
        var usuario = new Usuario
        {
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = "hash_fake",
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _service.EmailExistsAsync("teste@mottu.com");

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_ComEmailInexistente_DeveRetornarFalse()
    {
        // Act
        var resultado = await _service.EmailExistsAsync("email_inexistente@mottu.com");

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_ComEmailCaseInsensitive_DeveFuncionar()
    {
        // Arrange
        var usuario = new Usuario
        {
            Nome = "Teste Usuario",
            Email = "teste@mottu.com",
            Senha = "hash_fake",
            Perfil = Guid.NewGuid(),
            Ativo = true,
            DtCriacao = DateTime.UtcNow,
            DtAlteracao = DateTime.UtcNow
        };

        _context.Usuario.Add(usuario);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _service.EmailExistsAsync("TESTE@MOTTU.COM");

        // Assert
        resultado.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
    }
}




