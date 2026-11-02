using System.Text.Json.Serialization;

/// <summary>Representa os dados de uma despesa registrada.</summary>
public sealed record DespesaResponse(
    Guid Id,
    [property: JsonConverter(typeof(BrazilianDateTimeOffsetJsonConverter))]
    DateTimeOffset DataDespesa,
    string DescricaoDespesa,
    decimal ValorDespesa,
    string ModalidadePagamento,
    string CategoriaDespesa,
    string Ativo,
    DateTime Created);
