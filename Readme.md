
# 🏍️ PatioVision

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

## ✅ Exemplo de Fluxo

Cadastre um IOT de Pátio:

```http
POST /api/Dispositivos
Content-Type: application/json

{
  "tipo": "Patio",
  "ultimaLocalizacao": "Pátio Zona Leste",
  "ultimaAtualizacao": "2025-05-18T20:15:00Z"
}
```

Cadastre um Pátio:

```http
POST /api/Patios
Content-Type: application/json
{
  "nome": "Pátio Zona Leste",
  "categoria": "SemPlaca",
  "latitude": -23.5631,
  "longitude": -46.6544,
  "dispositivoIotId": "COLE_O_ID_RETORNADO_DO_DISPOSITIVO"
}
```

Cadastre um IOT de Moto:

```http
POST /api/Dispositivos
Content-Type: application/json
{
  "tipo": "Moto",
  "ultimaLocalizacao": "Rampa de saída",
  "ultimaAtualizacao": "2025-05-18T20:18:00Z"
}
```

Cadastre uma Moto:

```http
POST /api/Motos
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

📌 Autores:

- [Letícia Zago de Souza](https://www.linkedin.com/in/letícia-zago-de-souza)
- [Ana Carolina Reis Santana](https://www.linkedin.com/in/ana-carolina-santana-9a0a78232)
- [Celina Alcântara do Carmo](https://www.linkedin.com/in/celinaalcantara)
