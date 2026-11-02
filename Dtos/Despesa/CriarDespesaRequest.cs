using System.Text.Json.Serialization;

/// <summary>Dados necessários para registrar uma despesa.</summary>
public sealed record CriarDespesaRequest
{
    [JsonConverter(typeof(BrazilianDateTimeOffsetJsonConverter))]
    public required DateTimeOffset DataDespesa { get; init; }

    public required string DescricaoDespesa { get; init; }

    public required decimal ValorDespesa { get; init; }

    public required string ModalidadePagamento { get; init; }

    public required string CategoriaDespesa { get; init; }
}
