using System.Text.Json.Serialization;

/// <summary>Dados para substituir os dados de uma despesa existente.</summary>
public sealed record AtualizarDespesaRequest
{
    [JsonConverter(typeof(BrazilianDateTimeOffsetJsonConverter))]
    public required DateTimeOffset DataDespesa { get; init; }

    public required string DescricaoDespesa { get; init; }

    public required decimal ValorDespesa { get; init; }

    public required string ModalidadePagamento { get; init; }

    public required string CategoriaDespesa { get; init; }
}
