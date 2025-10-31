# 🤖 Endpoint de Otimização de Distribuição ML.NET

## 📌 Visão Geral

Foi implementado o endpoint de **Otimização de Distribuição de Motos por Pátio** usando **ML.NET**, que utiliza Machine Learning para recomendar a melhor redistribuição de motos entre pátios.

## 🎯 Funcionalidade

O endpoint analisa todas as motos disponíveis (ou as especificadas) e recomenda redistribuições que:
- Melhoram o equilíbrio entre pátios
- Potencializam a taxa de atendimento
- Reduzem congestionamento em pátios lotados
- Otimizam a localização baseada em padrões históricos

## 🔗 Endpoint

### POST `/api/v1/patios/otimizar-distribuicao`

**Autenticação:** Requerida (JWT Bearer Token)

**Request Body:**
```json
{
  "motoIds": null,              // Lista de IDs específicos ou null para todas
  "apenasDisponiveis": true,    // Considerar apenas motos com status Disponível
  "incluirTodasMotos": false    // Incluir motos já bem posicionadas
}
```

**Response (200 OK):**
```json
{
  "data": {
    "totalMotosAnalisadas": 5,
    "totalRecomendacoes": 3,
    "recomendacoes": [
      {
        "motoId": "guid",
        "modeloMoto": "PCX",
        "statusAtual": "Disponivel",
        "patioAtualId": "guid",
        "patioAtualNome": "Pátio Zona Leste",
        "patioRecomendadoId": "guid",
        "patioRecomendadoNome": "Pátio Zona Sul",
        "scoreAdequacao": 0.85,
        "melhoriaEsperada": 35.0,
        "motivo": "Pátio especializado em aluguel possui maior demanda; Melhor balanceamento de capacidade entre pátios",
        "beneficios": [
          "Redução estimada de 15-20% no tempo de atendimento",
          "Melhor distribuição da capacidade entre pátios",
          "Aumento da probabilidade de aluguel pela localização estratégica",
          "Score de adequação: 85.0%"
        ]
      }
    ],
    "estatisticas": {
      "beneficioEsperado": "Otimização de 3 moto(s) pode reduzir tempo de atendimento em até 20%",
      "reducaoTempoMedioAtendimento": 20.0,
      "melhoriaBalanceamentoPatio": 90.5,
      "patiosAfetados": 4
    }
  },
  "_links": {
    "self": {
      "href": "http://localhost:5000/api/v1/patios/otimizar-distribuicao",
      "method": "POST"
    }
  }
}
```

## 🧠 Modelo Machine Learning

### Algoritmo
- **Tipo:** Regression (FastTree)
- **Objetivo:** Predizer o score de adequação (0.0 a 1.0) de uma moto em um pátio

### Features Utilizadas

#### Features da Moto:
- Modelo da moto (encoded como categoria)
- Status atual (Disponivel, EmManutencao, Alugada)
- Dias desde cadastro
- Tempo médio em cada status

#### Features do Pátio:
- Categoria do pátio (SemPlaca, Manutencao, Aluguel)
- Localização geográfica (Latitude, Longitude)
- Capacidade do pátio
- Quantidade atual de motos
- Taxa de ocupação
- Taxa de aluguel histórica

#### Features Temporais:
- Dia da semana
- Mês do ano
- Hora do dia

### Treinamento
- **Dados Iniciais:** 1000 registros sintéticos baseados em heurísticas do domínio
- **Modelo:** Salvo em `MLModels/OtimizacaoDistribuicao.zip`
- **Nota:** Em produção, o modelo deve ser treinado com dados reais históricos

## 📁 Arquivos Criados/Modificados

### Novos Arquivos:
1. `PatioVision.Core/Models/ML/OtimizacaoDistribuicaoInput.cs` - Modelo de entrada ML
2. `PatioVision.Core/Models/ML/OtimizacaoDistribuicaoOutput.cs` - Modelo de saída ML
3. `PatioVision.API/DTOs/OtimizacaoDistribuicaoRequest.cs` - DTO de request
4. `PatioVision.API/DTOs/OtimizacaoDistribuicaoResponse.cs` - DTOs de response
5. `PatioVision.Service/Services/ML/OtimizacaoDistribuicaoService.cs` - Serviço ML
6. `PatioVision.API/Controllers/OtimizacaoDistribuicaoController.cs` - Controller

### Arquivos Modificados:
1. `PatioVision.Service/PatioVision.Service.csproj` - Adicionados pacotes ML.NET
2. `PatioVision.API/Program.cs` - Registrado o serviço ML

## 📦 Pacotes NuGet Adicionados

- `Microsoft.ML` (v3.0.1)
- `Microsoft.ML.FastTree` (v3.0.1)

## 🚀 Como Usar

### 1. Primeira Execução
Na primeira vez que o serviço for executado, o modelo ML será criado automaticamente com dados sintéticos.

### 2. Testar o Endpoint

```bash
# Exemplo usando curl
curl -X POST "https://localhost:5000/api/v1/patios/otimizar-distribuicao" \
  -H "Authorization: Bearer SEU_TOKEN_JWT" \
  -H "Content-Type: application/json" \
  -d '{
    "apenasDisponiveis": true
  }'
```

### 3. Exemplo com Swagger
1. Execute a aplicação
2. Acesse `http://localhost:{porta}/swagger`
3. Expanda o endpoint `POST /api/v1/patios/otimizar-distribuicao`
4. Clique em "Try it out"
5. Preencha o request body (ou deixe vazio para usar padrões)
6. Execute e veja as recomendações

## 🔧 Melhorias Futuras

1. **Treinamento com Dados Reais:**
   - Coletar histórico de redistribuições bem-sucedidas
   - Treinar modelo periodicamente com novos dados
   - Implementar re-treinamento automático

2. **Métricas de Avaliação:**
   - R² score do modelo
   - Mean Absolute Error (MAE)
   - Validação cruzada

3. **Features Adicionais:**
   - Distância geográfica entre pátios
   - Histórico de manutenções por modelo
   - Sazonalidade (eventos, feriados)

4. **Otimizações:**
   - Cache de predições
   - Processamento assíncrono para grandes volumes
   - API de re-treinamento sob demanda

## 📝 Notas Importantes

- O modelo atual usa dados sintéticos para demonstração
- Em produção, é essencial treinar com dados reais para melhor precisão
- O modelo é carregado na inicialização do serviço (singleton)
- Recomendações são geradas apenas se houver melhoria significativa (score > atual + 0.1)

## 🔍 Troubleshooting

**Erro ao carregar modelo:**
- Verifique se a pasta `MLModels` existe e tem permissões de escrita
- O modelo será recriado automaticamente se falhar ao carregar

**Nenhuma recomendação retornada:**
- Verifique se há motos disponíveis no banco
- Verifique se os pátios estão cadastrados
- As recomendações só aparecem se houver melhoria significativa

---

**Desenvolvido para PatioVision - Sistema de Rastreamento de Motocicletas**

