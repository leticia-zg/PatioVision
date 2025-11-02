using Microsoft.EntityFrameworkCore;
using PatioVision.Core.Enums;
using PatioVision.Core.Models;
using PatioVision.Data.Context;

namespace PatioVision.Data.Seeders;

/// <summary>
/// Seeder para gerar dados de treinamento ML com pelo menos 100 registros em cada tabela
/// </summary>
public class MlTrainingDataSeeder
{
    private readonly AppDbContext _context;
    private readonly Random _random;

    // Modelos de motos comuns
    private readonly string[] _modelosMotos = {
        "CG 160", "CG 150", "CG 125", "Pop 110i", "Biz 125", "NMax", "XRE 300", 
        "Twister 250", "CB 250", "Factor 150", "Fazer 250", "XTZ 125", "Bros 160"
    };

    // Nomes de pátios simulados
    private readonly string[] _nomesPatios = {
        "Pátio Centro", "Pátio Norte", "Pátio Sul", "Pátio Leste", "Pátio Oeste",
        "Pátio Shopping", "Pátio Terminal", "Pátio Aeroporto", "Pátio Universitário",
        "Pátio Industrial", "Pátio Comercial", "Pátio Residencial", "Pátio Praia",
        "Pátio Parque", "Pátio Rodoviária"
    };

    public MlTrainingDataSeeder(AppDbContext context)
    {
        _context = context;
        _random = new Random(42); // Seed fixa para dados consistentes
    }

    /// <summary>
    /// Executa o seed de todos os dados
    /// </summary>
    public async Task SeedAsync()
    {
        Console.WriteLine("Iniciando seed de dados de treinamento ML...");

        // Verifica se já existem dados (usando Count para compatibilidade com Oracle)
        var countDispositivos = await _context.Dispositivos.CountAsync();
        if (countDispositivos > 0)
        {
            Console.WriteLine("Dados já existem no banco. Pulando seed.");
            return;
        }

        // 1. Criar dispositivos IoT (200+ dispositivos: 100 para motos + 100+ para pátios)
        var dispositivos = await SeedDispositivosAsync();
        Console.WriteLine($"Criados {dispositivos.Count} dispositivos IoT");

        // 2. Criar pátios (100+ pátios)
        var patios = await SeedPatiosAsync(dispositivos.Where(d => d.Tipo == TipoDispositivo.Patio).ToList());
        Console.WriteLine($"Criados {patios.Count} pátios");

        // 3. Criar motos (100+ motos)
        var motos = await SeedMotosAsync(
            patios,
            dispositivos.Where(d => d.Tipo == TipoDispositivo.Moto).ToList()
        );
        Console.WriteLine($"Criadas {motos.Count} motos");

        // 4. Criar usuários (100+ usuários)
        var usuarios = await SeedUsuariosAsync();
        Console.WriteLine($"Criados {usuarios.Count} usuários");

        await _context.SaveChangesAsync();
        Console.WriteLine("Seed concluído com sucesso!");
    }

    /// <summary>
    /// Cria dispositivos IoT (mínimo 200: 100 para motos + 100+ para pátios)
    /// </summary>
    private async Task<List<DispositivoIoT>> SeedDispositivosAsync()
    {
        var dispositivos = new List<DispositivoIoT>();
        var baseDate = DateTime.UtcNow.AddMonths(-12); // Dados dos últimos 12 meses

        // 150 dispositivos para motos
        for (int i = 0; i < 150; i++)
        {
            var dispositivo = new DispositivoIoT
            {
                DispositivoIotId = Guid.NewGuid(),
                Tipo = TipoDispositivo.Moto,
                UltimaLocalizacao = $"Lat:-{23.5 + _random.NextDouble() * 0.5},Lng:-{46.6 + _random.NextDouble() * 0.5}",
                UltimaAtualizacao = baseDate.AddDays(_random.Next(0, 365)),
                DtCadastro = baseDate.AddDays(_random.Next(0, 365)),
                DtAtualizacao = _random.Next(0, 100) > 30 ? baseDate.AddDays(_random.Next(0, 365)) : null
            };
            dispositivos.Add(dispositivo);
        }

        // 120 dispositivos para pátios
        for (int i = 0; i < 120; i++)
        {
            var dispositivo = new DispositivoIoT
            {
                DispositivoIotId = Guid.NewGuid(),
                Tipo = TipoDispositivo.Patio,
                UltimaLocalizacao = $"Lat:-{23.5 + _random.NextDouble() * 0.5},Lng:-{46.6 + _random.NextDouble() * 0.5}",
                UltimaAtualizacao = baseDate.AddDays(_random.Next(0, 365)),
                DtCadastro = baseDate.AddDays(_random.Next(0, 365)),
                DtAtualizacao = _random.Next(0, 100) > 30 ? baseDate.AddDays(_random.Next(0, 365)) : null
            };
            dispositivos.Add(dispositivo);
        }

        _context.Dispositivos.AddRange(dispositivos);
        await _context.SaveChangesAsync();

        return dispositivos;
    }

    /// <summary>
    /// Cria pátios (mínimo 100) com diferentes categorias e localizações
    /// </summary>
    private async Task<List<Patio>> SeedPatiosAsync(List<DispositivoIoT> dispositivosPatio)
    {
        var patios = new List<Patio>();
        var baseDate = DateTime.UtcNow.AddMonths(-12);
        var categorias = Enum.GetValues<CategoriaPatio>();

        // Coordenadas aproximadas de São Paulo para variar localizações
        var centroLat = -23.5505m;
        var centroLng = -46.6333m;

        for (int i = 0; i < 100; i++)
        {
            var dispositivo = dispositivosPatio[_random.Next(dispositivosPatio.Count)];
            var nome = _nomesPatios[_random.Next(_nomesPatios.Length)] + $" {i + 1}";
            var categoria = categorias[_random.Next(categorias.Length)];

            // Distribuir pátios em diferentes áreas (simular diferentes localizações)
            var latOffset = (decimal)(_random.NextDouble() * 0.2 - 0.1); // ±0.1 graus (~11km)
            var lngOffset = (decimal)(_random.NextDouble() * 0.2 - 0.1);

            // Definir capacidade baseada na categoria do pátio
            int capacidade;
            switch (categoria)
            {
                case CategoriaPatio.Aluguel:
                    capacidade = _random.Next(15, 31); // 15-30 motos
                    break;
                case CategoriaPatio.Manutencao:
                    capacidade = _random.Next(5, 16); // 5-15 motos
                    break;
                case CategoriaPatio.SemPlaca:
                default:
                    capacidade = _random.Next(10, 26); // 10-25 motos
                    break;
            }

            var patio = new Patio
            {
                PatioId = Guid.NewGuid(),
                Nome = nome,
                Categoria = categoria,
                Latitude = centroLat + latOffset,
                Longitude = centroLng + lngOffset,
                Capacidade = capacidade,
                DispositivoIotId = dispositivo.DispositivoIotId,
                DtCadastro = baseDate.AddDays(_random.Next(0, 365)),
                DtAtualizacao = _random.Next(0, 100) > 40 ? baseDate.AddDays(_random.Next(0, 365)) : null
            };

            patios.Add(patio);
        }

        _context.Patios.AddRange(patios);
        await _context.SaveChangesAsync();

        return patios;
    }

    /// <summary>
    /// Cria motos (mínimo 100) com distribuição desequilibrada entre pátios
    /// </summary>
    private async Task<List<Moto>> SeedMotosAsync(List<Patio> patios, List<DispositivoIoT> dispositivosMoto)
    {
        var motos = new List<Moto>();
        var baseDate = DateTime.UtcNow.AddMonths(-12);
        var status = Enum.GetValues<StatusMoto>();

        // Simular distribuição desequilibrada: alguns pátios terão mais motos
        // 30% dos pátios terão 40% das motos (congestionados)
        // 70% dos pátios terão 60% das motos (menos congestionados)
        var patiosCongestionados = patios.Take((int)(patios.Count * 0.3)).ToList();
        var patiosNormais = patios.Skip((int)(patios.Count * 0.3)).ToList();

        int motoIndex = 0;

        // Criar 140 motos para garantir desequilíbrio
        for (int i = 0; i < 140; i++)
        {
            Patio patio;
            if (i < 60 && patiosCongestionados.Any())
            {
                // Primeiras 60 motos vão para pátios congestionados
                patio = patiosCongestionados[_random.Next(patiosCongestionados.Count)];
            }
            else
            {
                // Restante vai para pátios normais
                patio = patiosNormais[_random.Next(patiosNormais.Count)];
            }

            var dispositivo = dispositivosMoto[_random.Next(dispositivosMoto.Count)];
            var modelo = _modelosMotos[_random.Next(_modelosMotos.Length)];
            var statusMoto = status[_random.Next(status.Length)];

            // Algumas motos sem placa (30%)
            string? placa = null;
            if (_random.Next(0, 100) > 30)
            {
                placa = GeneratePlaca();
            }

            var moto = new Moto
            {
                MotoId = Guid.NewGuid(),
                Modelo = modelo,
                Placa = placa,
                Status = statusMoto,
                PatioId = patio.PatioId,
                DispositivoIotId = dispositivo.DispositivoIotId,
                DtCadastro = baseDate.AddDays(_random.Next(0, 365)),
                DtAtualizacao = _random.Next(0, 100) > 40 ? baseDate.AddDays(_random.Next(0, 365)) : null
            };

            motos.Add(moto);
            motoIndex++;
        }

        _context.Motos.AddRange(motos);
        await _context.SaveChangesAsync();

        return motos;
    }

    /// <summary>
    /// Cria usuários (mínimo 100)
    /// </summary>
    private async Task<List<Usuario>> SeedUsuariosAsync()
    {
        var usuarios = new List<Usuario>();
        var baseDate = DateTime.UtcNow.AddMonths(-12);

        // Prefixos de nomes para variar
        var primeirosNomes = new[] { "João", "Maria", "Pedro", "Ana", "Carlos", "Fernanda", "Ricardo", "Juliana" };
        var sobrenomes = new[] { "Silva", "Santos", "Oliveira", "Souza", "Pereira", "Costa", "Ferreira", "Almeida" };

        for (int i = 0; i < 100; i++)
        {
            var primeiroNome = primeirosNomes[_random.Next(primeirosNomes.Length)];
            var sobrenome = sobrenomes[_random.Next(sobrenomes.Length)];
            var nome = $"{primeiroNome} {sobrenome}";
            var email = $"{primeiroNome.ToLower()}.{sobrenome.ToLower()}{i}@patiovision.com";

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Email = email,
                Senha = BCrypt.Net.BCrypt.HashPassword("Senha123!"), // Senha padrão para testes
                Perfil = Guid.NewGuid(), // Perfis aleatórios para simulação
                DtCriacao = baseDate.AddDays(_random.Next(0, 365)),
                DtAlteracao = baseDate.AddDays(_random.Next(0, 365)),
                Ativo = _random.Next(0, 100) > 10 // 90% ativos
            };

            usuarios.Add(usuario);
        }

        _context.Usuario.AddRange(usuarios);
        await _context.SaveChangesAsync();

        return usuarios;
    }

    /// <summary>
    /// Gera uma placa aleatória no formato brasileiro (ABC1D23)
    /// </summary>
    private string GeneratePlaca()
    {
        var letras = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var numeros = "0123456789";

        var placa = new char[7];
        placa[0] = letras[_random.Next(letras.Length)];
        placa[1] = letras[_random.Next(letras.Length)];
        placa[2] = letras[_random.Next(letras.Length)];
        placa[3] = numeros[_random.Next(numeros.Length)];
        placa[4] = letras[_random.Next(letras.Length)];
        placa[5] = numeros[_random.Next(numeros.Length)];
        placa[6] = numeros[_random.Next(numeros.Length)];

        return new string(placa);
    }
}

