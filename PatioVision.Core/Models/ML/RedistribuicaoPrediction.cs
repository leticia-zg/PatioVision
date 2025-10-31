using Microsoft.ML.Data;

namespace PatioVision.Core.Models.ML;

/// <summary>
/// Predição do modelo ML.NET
/// </summary>
public class RedistribuicaoPrediction
{
    [ColumnName("Score")]
    public float Score { get; set; }
}

