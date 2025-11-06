# 🏍️ PatioVision

## ✍️ Integrantes

- [Letícia Zago de Souza](https://www.linkedin.com/in/letícia-zago-de-souza)
- [Ana Carolina Reis Santana](https://www.linkedin.com/in/ana-carolina-santana-9a0a78232)
- [Celina Alcântara do Carmo](https://www.linkedin.com/in/celinaalcantara)

---

## 📑 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Aplicação Interna](#-aplicação-interna)
- [Arquitetura e Tecnologias](#-arquitetura-e-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação e Configuração](#-instalação-e-configuração)
- [Autenticação](#-autenticação)
- [Rotas da API](#-rotas-da-api)
- [Exemplo de Fluxo Básico](#-exemplo-de-fluxo-básico)
- [Tutorial: Redistribuição ML](#-tutorial-redistribuição-ml)
- [Executando os Testes](#-executando-os-testes)
- [Como Contribuir](#-como-contribuir)
- [Licença](#-licença)

---

## 📌 Sobre o Projeto

O **PatioVision** é uma aplicação que permite o rastreamento e gerenciamento de motocicletas estacionadas em diferentes pátios, por meio de dispositivos IoT. A solução facilita a localização das motos, especialmente em ambientes de grande movimentação, como centros logísticos, estacionamentos e áreas de manutenção.

### Características Principais

- 🎯 **Rastreamento em Tempo Real**: Localização de motos através de dispositivos IoT
- 🤖 **Machine Learning**: Recomendações inteligentes de redistribuição de motos entre pátios
- 🔐 **Autenticação Segura**: Sistema de autenticação JWT com BCrypt
- 📊 **Monitoramento**: Health checks para verificação de saúde da API
- 📝 **Documentação**: Swagger/OpenAPI integrado

---

## 🏢 Aplicação Interna

Esta solução foi desenvolvida para uso exclusivo nos pátios da **Mottu**, com o objetivo de facilitar a localização e gestão de motocicletas em ambientes controlados. O sistema permite que operadores da Mottu visualizem, atualizem e rastreiem motos com base em dados coletados por dispositivos IoT alocados nas motos e nos pátios.

---

## 🏗️ Arquitetura e Tecnologias

### Estrutura do Projeto

```
PatioVision/
├── PatioVision.API/              # Camada de apresentação (Controllers, DTOs)
├── PatioVision.Service/          # Camada de serviços e lógica de negócio
├── PatioVision.Data/             # Camada de acesso a dados (DbContext, Migrations)
├── PatioVision.Core/             # Modelos, Enums, Entidades
├── PatioVision.Service.Tests/   # Testes unitários
├── PatioVision.API.Tests/        # Testes de integração
└── README.md                     # Este arquivo
```

### Stack Tecnológica

| Categoria | Tecnologia |
|-----------|-----------|
| **Framework** | ASP.NET Core 10.0 |
| **ORM** | Entity Framework Core 9.0 |
| **Banco de Dados** | Oracle Database |
| **Autenticação** | JWT Bearer Token |
| **Machine Learning** | ML.NET 3.0.1 com FastTree Regression |
| **Health Checks** | AspNetCore.HealthChecks.Oracle 9.0 |
| **API Versioning** | Asp.Versioning.Mvc 8.1.0 |
| **Documentação** | Swagger/OpenAPI 3.0 |
| **Testes Unitários** | xUnit, Moq, FluentAssertions |
| **Testes Integração** | Microsoft.AspNetCore.Mvc.Testing |

### Boas Práticas Implementadas

- ✅ **Clean Architecture** com separação de responsabilidades
- ✅ **Repository Pattern** através de Services
- ✅ **DTOs** para transferência de dados
- ✅ **Validação de entrada** com Data Annotations
- ✅ **Tratamento de erros** centralizado
- ✅ **Logging** para debug e monitoramento
- ✅ **Documentação XML** para Swagger
- ✅ **HATEOAS** em respostas paginadas
- ✅ **Migrações** para versionamento de schema
- ✅ **AsNoTracking()** para performance em consultas read-only
- ✅ **Testes unitários e de integração** com cobertura adequada

---

## 📋 Pré-requisitos

- .NET SDK 10.0 ou superior
- Banco de dados Oracle em funcionamento
- Ferramenta de acesso ao Oracle (DBeaver, SQL Developer, etc.)

---

## ⚙️ Instalação e Configuração

### 1. Clonar o Repositório

```bash
git clone https://github.com/leticia-zg/PatioVision.git
cd PatioVision
```

### 2. Configurar o Banco de Dados Oracle

Edite o arquivo `appsettings.json` do projeto `PatioVision.API` com a sua string de conexão Oracle:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=seu_usuario;Password=sua_senha;Data Source=//localhost:1521/XEPDB1;"
  },
  "Jwt": {
    "Key": "SuaChaveSecretaSuperSeguraComPeloMenos32Caracteres",
    "Issuer": "PatioVisionAPI",
    "Audience": "PatioVisionUsers"
  }
}
```

### 3. Aplicar Migrations e Iniciar a Aplicação

```bash
dotnet ef database update -p PatioVision.Data -s PatioVision.API
dotnet run --project PatioVision.API
```

### 4. Acessar a Documentação Swagger

Após iniciar a aplicação, acesse:

```
http://localhost:{porta}/swagger
```

---

## 🔐 Autenticação

A API utiliza **JWT (JSON Web Tokens)** para autenticação. A maioria dos endpoints requer autenticação via Bearer Token.

### 1. Criar um Usuário (Opcional - permite criação pública)

```http
POST /api/v1/usuarios
Content-Type: application/json

{
  "nome": "João Silva",
  "email": "joao.silva@mottu.com",
  "senha": "MinhaSenh@123",
  "perfil": "Operador",
  "ativo": true
}
```

**Resposta:**
```json
{
  "data": {
    "id": "uuid-do-usuario",
    "nome": "João Silva",
    "email": "joao.silva@mottu.com"
  },
  "links": {
    "self": {"href": "/api/v1/usuarios/uuid-do-usuario", "method": "GET"}
  }
}
```

### 2. Realizar Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "email": "joao.silva@mottu.com",
  "senha": "MinhaSenh@123"
}
```

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresIn": 28800,
  "usuarioId": "uuid-do-usuario",
  "nome": "João Silva",
  "email": "joao.silva@mottu.com",
  "perfilId": "Operador"
}
```

### 3. Usar o Token nas Requisições

Adicione o header em todas as requisições que requerem autenticação:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**⚠️ Importante:**
- O token JWT expira em 480 minutos (8 horas)
- A maioria dos endpoints requer autenticação via `[Authorize]`
- Apenas `/api/v1/auth/login`, `/api/v1/usuarios` (POST) e `/api/v1/seeder/*` são públicos

---

## 🔗 Rotas da API

Todas as rotas da API seguem o padrão `/api/v1/{recurso}`.

### 🛵 Motos

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| GET | `/api/v1/motos` | Lista todas as motos (com paginação) | 200 OK |
| GET | `/api/v1/motos/{id}` | Retorna uma moto específica pelo ID | 200 OK / 404 Not Found |
| POST | `/api/v1/motos` | Cadastra uma nova moto | 201 Created / 400 Bad Request |
| PUT | `/api/v1/motos/{id}` | Atualiza uma moto existente | 204 No Content / 400 / 404 |
| DELETE | `/api/v1/motos/{id}` | Remove uma moto | 204 No Content / 404 |

**Parâmetros de Consulta (GET):**
- `pageNumber`: Número da página (padrão: 1)
- `pageSize`: Tamanho da página, entre 1 e 100 (padrão: 10)
- `search`: Filtro por modelo ou placa
- `sort`: Ordenação (ex: `modelo`, `-modelo`, `dtcadastro`, `-dtcadastro`)

### 🏢 Pátios

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| GET | `/api/v1/patios` | Retorna todos os pátios cadastrados | 200 OK |
| GET | `/api/v1/patios/{id}` | Detalha um pátio específico | 200 OK / 404 Not Found |
| POST | `/api/v1/patios` | Cria um novo pátio | 201 Created / 400 Bad Request |
| PUT | `/api/v1/patios/{id}` | Atualiza um pátio | 204 No Content / 400 / 404 |
| DELETE | `/api/v1/patios/{id}` | Remove um pátio | 204 No Content / 404 |

### 📡 Dispositivos IoT

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| GET | `/api/v1/dispositivos` | Lista todos os dispositivos IoT | 200 OK |
| GET | `/api/v1/dispositivos/{id}` | Detalha um dispositivo específico | 200 OK / 404 Not Found |
| POST | `/api/v1/dispositivos` | Cadastra um novo dispositivo IoT | 201 Created / 400 Bad Request |
| PATCH | `/api/v1/dispositivos/{id}/localizacao` | Atualiza a localização do dispositivo | 200 OK / 404 Not Found |
| DELETE | `/api/v1/dispositivos/{id}` | Remove um dispositivo IoT | 204 No Content / 404 |

### 👥 Usuários

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| GET | `/api/v1/usuarios` | Lista todos os usuários | 200 OK |
| GET | `/api/v1/usuarios/{id}` | Retorna um usuário específico | 200 OK / 404 Not Found |
| POST | `/api/v1/usuarios` | Registra um novo usuário | 201 Created / 400 Bad Request |
| PUT | `/api/v1/usuarios/{id}` | Atualiza um usuário existente | 200 OK / 400 / 404 |
| DELETE | `/api/v1/usuarios/{id}` | Remove um usuário | 204 No Content / 404 |

### 🔐 Autenticação

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| POST | `/api/v1/auth/login` | Realiza login e retorna token JWT | 200 OK / 401 Unauthorized |

### ❤️ Health Checks

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| GET | `/health` | Verifica saúde geral da API | 200 OK / 503 Service Unavailable |
| GET | `/health/live` | Verifica se a API está viva | 200 OK |
| GET | `/health/ready` | Verifica se a API está pronta (Oracle OK) | 200 OK / 503 Service Unavailable |

### 🤖 ML - Redistribuição de Motos

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| POST | `/api/v1/redistribuicao/recomendar` | Gera recomendações de redistribuição usando ML.NET | 200 OK / 400 / 500 |

### 🌱 Seeder de Dados

| Método | Rota | Descrição | Status HTTP |
|--------|------|-----------|-------------|
| POST | `/api/v1/seeder/ml-training-data` | Popula banco com dados de treinamento ML | 200 OK / 500 |

---

## ✅ Exemplo de Fluxo Básico

**Nota:** Todos os exemplos abaixo assumem que você já obteve um token JWT válido através do endpoint `/api/v1/auth/login`.

### 1. Cadastre um Dispositivo IoT de Pátio

```http
POST /api/v1/dispositivos
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "tipo": "Patio",
  "ultimaLocalizacao": "Pátio Zona Leste",
  "ultimaAtualizacao": "2025-05-18T20:15:00Z"
}
```

### 2. Cadastre um Pátio

```http
POST /api/v1/patios
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "nome": "Pátio Zona Leste",
  "categoria": "SemPlaca",
  "latitude": -23.5631,
  "longitude": -46.6544,
  "capacidade": 50,
  "dispositivoIotId": "COLE_O_ID_RETORNADO_DO_DISPOSITIVO"
}
```

### 3. Cadastre um Dispositivo IoT de Moto

```http
POST /api/v1/dispositivos
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "tipo": "Moto",
  "ultimaLocalizacao": "Rampa de saída",
  "ultimaAtualizacao": "2025-05-18T20:18:00Z"
}
```

### 4. Cadastre uma Moto

```http
POST /api/v1/motos
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "modelo": "PCX",
  "placa": "FSR-8Y34",
  "status": "Disponivel",
  "patioId": "COLE_O_ID_RETORNADO_DO_PATIO",
  "dispositivoIotId": "COLE_O_ID_RETORNADO_DO_DISPOSITIVO_DA_MOTO"
}
```

---

## 🤖 Tutorial: Redistribuição ML

Este tutorial mostra como usar os recursos de Machine Learning para obter recomendações de redistribuição de motos entre pátios.

### Passo 1: Popular Dados de Treinamento

Antes de usar o endpoint de redistribuição, é necessário popular o banco de dados com dados de treinamento. O seeder cria:

- **270 dispositivos IoT** (150 para motos + 120 para pátios)
- **100 pátios** com diferentes categorias e localizações geográficas
- **140 motos** distribuídas de forma desequilibrada entre os pátios
- **100 usuários** com perfis variados

**Executar o seeder:**
```http
POST /api/v1/seeder/ml-training-data
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json
```

**Resposta de sucesso:**
```json
{
  "message": "Seeder executado com sucesso. Dados de treinamento ML foram populados no banco de dados.",
  "timestamp": "2025-01-28T10:30:00Z"
}
```

**⚠️ Importante:**
- O seeder verifica se já existem dados no banco. Se houver dados, o seed será pulado automaticamente.
- Execute este endpoint apenas uma vez, ou quando desejar resetar os dados de treinamento.
- Este processo pode levar alguns segundos devido à quantidade de dados criados.

### Passo 2: Obter Recomendações de Redistribuição

Após popular os dados, você pode usar o endpoint de redistribuição para obter recomendações baseadas em ML.NET.

**Recomendação para todas as motos disponíveis:**
```http
POST /api/v1/redistribuicao/recomendar
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{}
```

**Recomendação para motos específicas:**
```http
POST /api/v1/redistribuicao/recomendar
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "motoIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "4fa85f64-5717-4562-b3fc-2c963f66afa7"
  ]
}
```

**Recomendação considerando apenas pátios específicos:**
```http
POST /api/v1/redistribuicao/recomendar
Authorization: Bearer {seu_token_jwt}
Content-Type: application/json

{
  "motoIds": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"],
  "patioIds": [
    "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "6fa85f64-5717-4562-b3fc-2c963f66afa9"
  ]
}
```

### Passo 3: Entender a Resposta

A resposta do endpoint inclui recomendações ordenadas por score e métricas detalhadas:

```json
{
  "recomendacoes": [
    {
      "motoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "motoModelo": "CG 160",
      "motoPlaca": "ABC1D23",
      "patioOrigemId": "7fa85f64-5717-4562-b3fc-2c963f66afa1",
      "patioOrigemNome": "Pátio Centro 1",
      "score": 0.95,
      "patioDestinoId": "8fa85f64-5717-4562-b3fc-2c963f66afa2",
      "patioDestinoNome": "Pátio Norte 5",
      "motivos": [
        "Pátio origem está congestionado (25 motos)",
        "Pátio destino tem capacidade disponível (8 motos)",
        "Melhora significativa no equilíbrio de distribuição"
      ],
      "impactoEquilibrio": 0.75
    }
  ],
  "metricas": {
    "totalMotos": 140,
    "totalPatios": 100,
    "mediaMotosPorPatio": 1.4,
    "desvioPadraoAtual": 2.8,
    "desvioPadraoEstimado": 1.2,
    "melhoriaEquilibrioPercentual": 57.14,
    "patiosCongestionados": 15,
    "patiosSubutilizados": 35
  },
  "totalRecomendacoes": 50
}
```

### Interpretando os Resultados

- **Score**: Valor de 0 a 1, onde 1 é a melhor recomendação. Priorize recomendações com score > 0.7
- **ImpactoEquilibrio**: Quanto maior, melhor a melhoria no equilíbrio de distribuição
- **Motivos**: Explicações em texto sobre por que a redistribuição é recomendada
- **Métricas**:
  - `desvioPadraoAtual`: Quanto maior, mais desequilibrada está a distribuição atual
  - `desvioPadraoEstimado`: Distribuição esperada após aplicar as recomendações
  - `melhoriaEquilibrioPercentual`: Percentual de melhoria esperado no equilíbrio

### Como Funciona o Modelo ML

O modelo ML.NET utiliza um algoritmo **FastTree** (regressão) que aprende padrões de distribuição baseados em:

1. **Equilíbrio entre pátios**: Distribuição uniforme do número de motos
2. **Distância geográfica**: Calculada usando fórmula de Haversine
3. **Categoria do pátio**: Compatibilidade com o status da moto
4. **Taxa de ocupação**: Relação entre ocupação atual e média geral
5. **Congestionamento**: Identificação de pátios acima da capacidade média

O modelo é treinado automaticamente na primeira chamada ao endpoint, usando os dados do banco como base.

---

## 🧪 Executando os Testes

O projeto possui dois tipos de testes: **testes unitários** e **testes de integração**.

### Testes Unitários

Os testes unitários estão localizados no projeto `PatioVision.Service.Tests` e testam a lógica principal dos serviços utilizando banco de dados em memória.

**Executar todos os testes unitários:**
```bash
dotnet test PatioVision.Service.Tests
```

**Executar testes unitários com output detalhado:**
```bash
dotnet test PatioVision.Service.Tests --logger "console;verbosity=detailed"
```

**Executar testes de um arquivo específico:**
```bash
dotnet test PatioVision.Service.Tests --filter "FullyQualifiedName~MotoServiceTests"
```

### Testes de Integração

Os testes de integração estão localizados no projeto `PatioVision.API.Tests` e testam os endpoints da API de forma completa, incluindo autenticação, controllers e serviços.

**Executar todos os testes de integração:**
```bash
dotnet test PatioVision.API.Tests
```

**Executar testes de integração com output detalhado:**
```bash
dotnet test PatioVision.API.Tests --logger "console;verbosity=detailed"
```

**Executar testes de um controller específico:**
```bash
dotnet test PatioVision.API.Tests --filter "FullyQualifiedName~AuthControllerTests"
```

### Executar Todos os Testes

Para executar todos os testes (unitários e de integração) de uma vez:

```bash
dotnet test
```

### Observações Importantes

- Os testes de integração utilizam banco de dados **InMemory** do Entity Framework Core, não sendo necessário ter o Oracle configurado para executar os testes
- Os testes são executados em ambientes isolados, cada teste tem seu próprio contexto de banco de dados
- Os testes de integração que requerem autenticação criam automaticamente um usuário de teste e obtêm um token JWT válido

### Estrutura dos Testes

**Testes Unitários (`PatioVision.Service.Tests`):**
- `MotoServiceTests.cs` - Testa CRUD, paginação, filtros e validações do serviço de motos
- `AuthServiceTests.cs` - Testa autenticação, geração de JWT e validação de email
- `RedistribuicaoServiceTests.cs` - Testa cálculo de métricas e recomendações de redistribuição

**Testes de Integração (`PatioVision.API.Tests`):**
- `AuthControllerTests.cs` - Testa endpoint de login com fluxo completo
- `MotosControllerTests.cs` - Testa endpoints CRUD de motos com autenticação

---

## 🚀 Como Contribuir

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 👥 Equipe de Desenvolvimento

Desenvolvido com 💚 por:
- **Letícia Zago de Souza** - [LinkedIn](https://www.linkedin.com/in/letícia-zago-de-souza)
- **Ana Carolina Reis Santana** - [LinkedIn](https://www.linkedin.com/in/ana-carolina-santana-9a0a78232)
- **Celina Alcântara do Carmo** - [LinkedIn](https://www.linkedin.com/in/celinaalcantara)
