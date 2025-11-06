using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatioVision.Data.Context;

namespace PatioVision.API.Tests.IntegrationTests;

/// <summary>
/// Factory personalizada para configurar a aplicação de teste
/// Substitui o banco Oracle por InMemory para testes de integração
/// </summary>
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    // Usar um nome de banco fixo por instância para garantir que todos os contextos compartilhem os mesmos dados
    private readonly string _databaseName = $"PatioVision_Test_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Garantir que o ambiente seja Test ANTES de qualquer configuração
        builder.UseEnvironment("Test");

        // Configurar variável de ambiente para garantir que seja Test
        builder.ConfigureAppConfiguration((context, config) =>
        {
            context.HostingEnvironment.EnvironmentName = "Test";
        });

        builder.ConfigureServices(services =>
        {
            // Remover TODOS os registros relacionados ao DbContext
            // Primeiro remover o DbContext registrado
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // Remover DbContextOptions que pode conter referência ao Oracle provider
            var optionsDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            
            if (optionsDescriptor != null)
            {
                services.Remove(optionsDescriptor);
            }

            // Adicionar DbContext com InMemory para testes
            // Usar o mesmo nome de banco para garantir que todos os contextos compartilhem os dados
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }
}

