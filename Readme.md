<<<<<<< HEAD

# 🏍️ PatioVision

## ✍️ Integrantes:

- [Letícia Zago de Souza](https://www.linkedin.com/in/letícia-zago-de-souza)
- [Ana Carolina Reis Santana](https://www.linkedin.com/in/ana-carolina-santana-9a0a78232)
- [Celina Alcântara do Carmo](https://www.linkedin.com/in/celinaalcantara)

---

## 📌 Sobre o Projeto

O **PatioVision** é uma aplicação que permite o rastreamento e gerenciamento de motocicletas estacionadas em diferentes pátios, por meio de dispositivos IoT.  
A solução facilita a localização das motos, especialmente em ambientes de grande movimentação, como centros logísticos, estacionamentos e áreas de manutenção.

O projeto utiliza uma arquitetura em camadas, com:

- API RESTful desenvolvida em ASP.NET Core
- Camada Core com modelos e enums
- Persistência com Entity Framework Core e banco de dados Oracle

---

## 🏢 Aplicação Interna

Esta solução foi desenvolvida para uso exclusivo nos pátios da **Mottu**, com o objetivo de facilitar a localização e gestão de motocicletas em ambientes controlados.  
O sistema permite que operadores da Mottu visualizem, atualizem e rastreiem motos com base em dados coletados por dispositivos IoT alocados nas motos e nos pátios.

---

## 🔗 Rotas da API

### 🛵 Motos

| Método | Rota                      | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/motos`              | Lista todas as motos                       | 200 OK                        |
| GET    | `/api/motos/{id}`         | Retorna uma moto específica pelo ID        | 200 OK / 404 Not Found        |
| GET    | `/api/motos/status?valor=Disponivel` | Filtra motos por status         | 200 OK / 400 Bad Request      |
| POST   | `/api/motos`              | Cadastra uma nova moto                     | 201 Created / 400 Bad Request |
| PUT    | `/api/motos/{id}`         | Atualiza uma moto existente                | 204 No Content / 400 / 404    |
| DELETE | `/api/motos/{id}`         | Remove uma moto                            | 204 No Content / 404          |

### 🏢 Pátios

| Método | Rota                      | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/patios`             | Retorna todos os pátios cadastrados        | 200 OK                        |
| GET    | `/api/patios/{id}`        | Detalha um pátio específico                | 200 OK / 404 Not Found        |
| POST   | `/api/patios`             | Cria um novo pátio                     | 201 Created / 400 Bad Request |
| PUT    | `/api/patios/{id}`        | Atualiza um pátio                          | 204 No Content / 400 / 404    |
| DELETE | `/api/patios/{id}`        | Remove um pátio                            | 204 No Content / 404          |

### 📡 Dispositivos IoT

| Método | Rota                           | Descrição                                  | Status HTTP Esperado         |
|--------|--------------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/dispositivos`           | Lista todos os dispositivos IoT            | 200 OK                        |
| GET    | `/api/dispositivos/{id}`      | Detalha um dispositivo específico          | 200 OK / 404 Not Found        |
| PATCH  | `/api/dispositivos/{id}/localizacao` | Atualiza a localização do dispositivo | 200 OK / 404 Not Found        |
| POST   | `/api/dispositivos`           | Cadastra um novo dispositivo IoT           | 201 Created / 400 Bad Request |
| DELETE | `/api/dispositivos/{id}`      | Remove um dispositivo IoT                  | 204 No Content / 404          |

### 🤖 ML - Redistribuição de Motos

| Método | Rota                                  | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------------------|--------------------------------------------|-------------------------------|
| POST   | `/api/v1/redistribuicao/recomendar`  | Gera recomendações de redistribuição usando ML.NET | 200 OK / 400 / 500 |

### 🌱 Seeder de Dados

| Método | Rota                          | Descrição                                  | Status HTTP Esperado         |
|--------|-------------------------------|--------------------------------------------|-------------------------------|
| POST   | `/api/v1/seeder/ml-training-data` | Popula banco com dados de treinamento ML | 200 OK / 500                  |

---

## 📋 Pré-requisitos

- .NET SDK 9.0 ou superior
- Banco de dados Oracle em funcionamento
- Ferramenta de acesso ao Oracle (DBeaver, SQL Developer, etc.)

---

## ⚙️ Como Instalar e Rodar o Projeto

### 1. Clonar o Repositório

```bash
git clone https://github.com/leticia-zg/PatioVision.git
cd PatioVision
```

### 2. Configurar o Banco de Dados Oracle

Edite o arquivo `appsettings.json` do projeto `PatioVision.API` com a sua string de conexão Oracle:

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=seu_usuario;Password=sua_senha;Data Source=//localhost:1521/XEPDB1;"
}
```

### 3. Aplicar Migrations e Iniciar a Aplicação

```bash
dotnet ef database update -p PatioVision.Data -s PatioVision.API
dotnet run --project PatioVision.API
```

### 4. Acessar a Documentação Swagger

```bash
http://localhost:{porta}/swagger
```

---

## ✅ Exemplo de Fluxo Básico

### 1. Cadastre um IOT de Pátio

```http
POST /api/dispositivos
Content-Type: application/json

{
  "tipo": "Patio",
  "ultimaLocalizacao": "Pátio Zona Leste",
  "ultimaAtualizacao": "2025-05-18T20:15:00Z"
}
```

### 2. Cadastre um Pátio

```http
POST /api/patios
Content-Type: application/json
{
  "nome": "Pátio Zona Leste",
  "categoria": "SemPlaca",
  "latitude": -23.5631,
  "longitude": -46.6544,
  "dispositivoIotId": "COLE_O_ID_RETORNADO_DO_DISPOSITIVO"
}
```

### 3. Cadastre um IOT de Moto

```http
POST /api/dispositivos
Content-Type: application/json
{
  "tipo": "Moto",
  "ultimaLocalizacao": "Rampa de saída",
  "ultimaAtualizacao": "2025-05-18T20:18:00Z"
}
```

### 4. Cadastre uma Moto

```http
POST /api/motos
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

## 🤖 Tutorial: Usando Redistribuição ML

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

A resposta do endpoint inclui:

1. **Recomendações**: Lista ordenada por score (melhores primeiro)
2. **Métricas**: Análise da distribuição atual vs proposta

**Exemplo de resposta:**

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

=======

# 🏍️ PatioVision

## ✍️ Integrantes:

- [Letícia Zago de Souza](https://www.linkedin.com/in/letícia-zago-de-souza)
- [Ana Carolina Reis Santana](https://www.linkedin.com/in/ana-carolina-santana-9a0a78232)
- [Celina Alcântara do Carmo](https://www.linkedin.com/in/celinaalcantara)

---

## 📌 Sobre o Projeto

O **PatioVision** é uma aplicação que permite o rastreamento e gerenciamento de motocicletas estacionadas em diferentes pátios, por meio de dispositivos IoT.  
A solução facilita a localização das motos, especialmente em ambientes de grande movimentação, como centros logísticos, estacionamentos e áreas de manutenção.

O projeto utiliza uma arquitetura em camadas, com:

- API RESTful desenvolvida em ASP.NET Core
- Camada Core com modelos e enums
- Persistência com Entity Framework Core e banco de dados Oracle

---

## 🏢 Aplicação Interna

Esta solução foi desenvolvida para uso exclusivo nos pátios da **Mottu**, com o objetivo de facilitar a localização e gestão de motocicletas em ambientes controlados.  
O sistema permite que operadores da Mottu visualizem, atualizem e rastreiem motos com base em dados coletados por dispositivos IoT alocados nas motos e nos pátios.

---

## 🔗 Rotas da API

### 🛵 Motos

| Método | Rota                      | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/motos`              | Lista todas as motos                       | 200 OK                        |
| GET    | `/api/motos/{id}`         | Retorna uma moto específica pelo ID        | 200 OK / 404 Not Found        |
| GET    | `/api/motos/status?valor=Disponivel` | Filtra motos por status         | 200 OK / 400 Bad Request      |
| POST   | `/api/motos`              | Cadastra uma nova moto                     | 201 Created / 400 Bad Request |
| PUT    | `/api/motos/{id}`         | Atualiza uma moto existente                | 204 No Content / 400 / 404    |
| DELETE | `/api/motos/{id}`         | Remove uma moto                            | 204 No Content / 404          |

### 🏢 Pátios

| Método | Rota                      | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/patios`             | Retorna todos os pátios cadastrados        | 200 OK                        |
| GET    | `/api/patios/{id}`        | Detalha um pátio específico                | 200 OK / 404 Not Found        |
| POST   | `/api/patios`             | Cria um novo pátio                     | 201 Created / 400 Bad Request |
| PUT    | `/api/patios/{id}`        | Atualiza um pátio                          | 204 No Content / 400 / 404    |
| DELETE | `/api/patios/{id}`        | Remove um pátio                            | 204 No Content / 404          |

### 📡 Dispositivos IoT

| Método | Rota                           | Descrição                                  | Status HTTP Esperado         |
|--------|--------------------------------|--------------------------------------------|-------------------------------|
| GET    | `/api/dispositivos`           | Lista todos os dispositivos IoT            | 200 OK                        |
| GET    | `/api/dispositivos/{id}`      | Detalha um dispositivo específico          | 200 OK / 404 Not Found        |
| PATCH  | `/api/dispositivos/{id}/localizacao` | Atualiza a localização do dispositivo | 200 OK / 404 Not Found        |
| POST   | `/api/dispositivos`           | Cadastra um novo dispositivo IoT           | 201 Created / 400 Bad Request |
| DELETE | `/api/dispositivos/{id}`      | Remove um dispositivo IoT                  | 204 No Content / 404          |

### 🤖 ML - Redistribuição de Motos

| Método | Rota                                  | Descrição                                  | Status HTTP Esperado         |
|--------|---------------------------------------|--------------------------------------------|-------------------------------|
| POST   | `/api/v1/redistribuicao/recomendar`  | Gera recomendações de redistribuição usando ML.NET | 200 OK / 400 / 500 |

### 🌱 Seeder de Dados

| Método | Rota                          | Descrição                                  | Status HTTP Esperado         |
|--------|-------------------------------|--------------------------------------------|-------------------------------|
| POST   | `/api/v1/seeder/ml-training-data` | Popula banco com dados de treinamento ML | 200 OK / 500                  |

---

## 📋 Pré-requisitos

- .NET SDK 9.0 ou superior
- Banco de dados Oracle em funcionamento
- Ferramenta de acesso ao Oracle (DBeaver, SQL Developer, etc.)

---

## ⚙️ Como Instalar e Rodar o Projeto

### 1. Clonar o Repositório

```bash
git clone https://github.com/leticia-zg/PatioVision.git
cd PatioVision
```

### 2. Configurar o Banco de Dados Oracle

Edite o arquivo `appsettings.json` do projeto `PatioVision.API` com a sua string de conexão Oracle:

```json
"ConnectionStrings": {
  "OracleConnection": "User Id=seu_usuario;Password=sua_senha;Data Source=//localhost:1521/XEPDB1;"
}
```

### 3. Aplicar Migrations e Iniciar a Aplicação

```bash
dotnet ef database update -p PatioVision.Data -s PatioVision.API
dotnet run --project PatioVision.API
```

### 4. Acessar a Documentação Swagger

```bash
http://localhost:{porta}/swagger
```

---

## ✅ Exemplo de Fluxo Básico

### 1. Cadastre um IOT de Pátio

```http
POST /api/dispositivos
Content-Type: application/json

{
  "tipo": "Patio",
  "ultimaLocalizacao": "Pátio Zona Leste",
  "ultimaAtualizacao": "2025-05-18T20:15:00Z"
}
```

### 2. Cadastre um Pátio

```http
POST /api/patios
Content-Type: application/json
{
  "nome": "Pátio Zona Leste",
  "categoria": "SemPlaca",
  "latitude": -23.5631,
  "longitude": -46.6544,
  "dispositivoIotId": "COLE_O_ID_RETORNADO_DO_DISPOSITIVO"
}
```

### 3. Cadastre um IOT de Moto

```http
POST /api/dispositivos
Content-Type: application/json
{
  "tipo": "Moto",
  "ultimaLocalizacao": "Rampa de saída",
  "ultimaAtualizacao": "2025-05-18T20:18:00Z"
}
```

### 4. Cadastre uma Moto

```http
POST /api/motos
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

## 🤖 Tutorial: Usando Redistribuição ML

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

A resposta do endpoint inclui:

1. **Recomendações**: Lista ordenada por score (melhores primeiro)
2. **Métricas**: Análise da distribuição atual vs proposta

**Exemplo de resposta:**

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

<<<<<<< Updated upstream
=======
>>>>>>> 05260f4b7ce0b7a95a5bd6fcd7688de7b9872a12
>>>>>>> Stashed changes
