using System.Text.Json.Serialization;

/// <summary>Dados opcionais para atualizar parcialmente uma despesa.</summary>
public sealed record AtualizarParcialDespesaRequest
{
    [JsonConverter(typeof(BrazilianNullableDateTimeOffsetJsonConverter))]
    public DateTimeOffset? DataDespesa { get; init; }

    public string? DescricaoDespesa { get; init; }

    public decimal? ValorDespesa { get; init; }

    public string? ModalidadePagamento { get; init; }

    public string? CategoriaDespesa { get; init; }
}
