/// <summary>Analisa imagens de comprovantes e extrai dados de despesas.</summary>
public interface IReceiptAnalysisService
{
    Task<ReceiptAnalysisResponse> AnalyzeAsync(
        Stream image,
        string contentType,
        CancellationToken cancellationToken);
}

/// <summary>Resposta da análise de um comprovante.</summary>
public sealed record ReceiptAnalysisResponse(ReceiptAnalysisDraft Draft);

/// <summary>Dados extraídos para preencher o rascunho de uma despesa.</summary>
public sealed record ReceiptAnalysisDraft(
    string DataDespesa,
    string DescricaoDespesa,
    string ValorDespesa,
    string ModalidadePagamento,
    string CategoriaDespesa);
