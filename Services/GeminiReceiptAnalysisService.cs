using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>Analisa comprovantes com a API Google Gemini.</summary>
public sealed class GeminiReceiptAnalysisService : IReceiptAnalysisService
{
    private const string Prompt = """
        Você é um assistente de IA especializado em extração de dados e OCR (Reconhecimento Óptico de Caracteres) de cupons fiscais, recibos e notas fiscais de despesas.

        Analise a imagem fornecida do comprovante e extraia as informações essenciais para preencher o rascunho de uma despesa.

        Você DEVE responder APENAS com um objeto JSON válido, sem markdown, sem explicações anteriores ou posteriores (não use blocos de código ```json).

        Regras de Extração e Formatação:
        1. "dataDespesa": Procure a data e a hora da emissão do comprovante. Converta estritamente para o formato "DD/MM/AAAA HH:MM:SS" (Exemplo: 29/10/2026 18:08:23). IMPORTANTE: Nunca inclua uma vírgula entre a data e a hora. Se a hora não for encontrada no recibo, use "12:00:00".
        2. "descricaoDespesa": Nome do estabelecimento comercial ou identificação clara do gasto (Exemplo: "Supermercado Zona Sul").
        3. "valorDespesa": Identifique o VALOR TOTAL pago no recibo. Formate como string utilizando vírgula para os centavos (Exemplo: "142,50"). Nunca inclua o símbolo "R$".
        4. "modalidadePagamento": Identifique a forma de pagamento descrita no recibo. Mapeie estritamente para uma das seguintes opções padrão: "PIX", "Cartão de débito", "Cartão de crédito", "Dinheiro", "Transferência" ou "Outro".
        5. "categoriaDespesa": Infira a categoria lógica do gasto com base no tipo de estabelecimento ou nos itens comprados. Mapeie estritamente para uma das seguintes opções: "Alimentação", "Transporte", "Moradia", "Saúde", "Lazer" ou "Outros".

        Exemplo de Saída Esperada:
        {
          "draft": {
            "dataDespesa": "29/10/2026 18:08:23",
            "descricaoDespesa": "Posto Ipiranga",
            "valorDespesa": "150,00",
            "modalidadePagamento": "Cartão de crédito",
            "categoriaDespesa": "Transporte"
          }
        }
        """;

    private static readonly JsonElement ResponseSchema = CreateResponseSchema();

    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiReceiptAnalysisService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GEMINI_API_KEY"] ?? configuration["Gemini:ApiKey"];
        _model = configuration["GEMINI_MODEL"] ?? configuration["Gemini:Model"] ?? "gemini-2.5-flash";
    }

    public async Task<ReceiptAnalysisResponse> AnalyzeAsync(
        Stream image,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "Configure a variável de ambiente GEMINI_API_KEY para analisar comprovantes.");
        }

        using var imageBuffer = new MemoryStream();
        await image.CopyToAsync(imageBuffer, cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(_model)}:generateContent");
        request.Headers.TryAddWithoutValidation("x-goog-api-key", _apiKey);
        request.Content = JsonContent.Create(new GeminiRequest(
            [
                new GeminiContent(
                    [
                        new GeminiPart { Text = Prompt },
                        new GeminiPart
                        {
                            InlineData = new GeminiInlineData(
                                contentType,
                                Convert.ToBase64String(imageBuffer.ToArray()))
                        }
                    ])
            ],
            new GeminiGenerationConfig(ResponseSchema)));

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"A API Gemini retornou HTTP {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        using var document = JsonDocument.Parse(responseBody);
        if (!document.RootElement.TryGetProperty("candidates", out var candidates)
            || candidates.GetArrayLength() == 0
            || !candidates[0].TryGetProperty("content", out var content)
            || !content.TryGetProperty("parts", out var parts))
        {
            throw new InvalidOperationException("A resposta do Gemini não contém dados extraídos.");
        }

        var jsonText = parts
            .EnumerateArray()
            .Where(part => part.TryGetProperty("text", out _))
            .Select(part => part.GetProperty("text").GetString())
            .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));

        if (jsonText is null)
        {
            throw new InvalidOperationException("A resposta do Gemini não contém o JSON do rascunho.");
        }

        return ParseDraft(jsonText);
    }

    private static ReceiptAnalysisResponse ParseDraft(string json)
    {
        using var document = JsonDocument.Parse(json);
        var draft = document.RootElement.GetProperty("draft");

        var dataDespesa = ReadRequiredString(draft, "dataDespesa");
        if (!DateTime.TryParseExact(
                dataDespesa,
                "dd/MM/yyyy HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
        {
            throw new InvalidOperationException("O Gemini retornou uma data fora do formato DD/MM/AAAA HH:MM:SS.");
        }

        var descricaoDespesa = ReadRequiredString(draft, "descricaoDespesa").Trim();
        if (descricaoDespesa.Length == 0)
        {
            throw new InvalidOperationException("O Gemini não identificou a descrição do comprovante.");
        }

        var valorTexto = ReadRequiredString(draft, "valorDespesa");
        if (!decimal.TryParse(valorTexto, NumberStyles.Number, BrazilianCulture, out var valorDespesa)
            || valorDespesa <= 0)
        {
            throw new InvalidOperationException("O Gemini retornou um valor total inválido.");
        }

        var modalidadePagamento = ReadRequiredString(draft, "modalidadePagamento");
        if (modalidadePagamento is not (
            "PIX" or "Cartão de débito" or "Cartão de crédito" or "Dinheiro" or "Transferência" or "Outro"))
        {
            throw new InvalidOperationException("O Gemini retornou uma modalidade de pagamento inválida.");
        }

        var categoriaDespesa = ReadRequiredString(draft, "categoriaDespesa");
        if (categoriaDespesa is not ("Alimentação" or "Transporte" or "Moradia" or "Saúde" or "Lazer" or "Outros"))
        {
            throw new InvalidOperationException("O Gemini retornou uma categoria de despesa inválida.");
        }

        return new ReceiptAnalysisResponse(new ReceiptAnalysisDraft(
            dataDespesa,
            descricaoDespesa,
            valorDespesa.ToString("0.00", BrazilianCulture),
            modalidadePagamento,
            categoriaDespesa));
    }

    private static string ReadRequiredString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property)
            || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException($"O Gemini não retornou o campo obrigatório {propertyName}.");
        }

        var value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"O Gemini não retornou o campo obrigatório {propertyName}.");
        }

        return value;
    }

    private static JsonElement CreateResponseSchema()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "type": "OBJECT",
              "properties": {
                "draft": {
                  "type": "OBJECT",
                  "properties": {
                    "dataDespesa": { "type": "STRING" },
                    "descricaoDespesa": { "type": "STRING" },
                    "valorDespesa": { "type": "STRING" },
                    "modalidadePagamento": {
                      "type": "STRING",
                      "enum": ["PIX", "Cartão de débito", "Cartão de crédito", "Dinheiro", "Transferência", "Outro"]
                    },
                    "categoriaDespesa": {
                      "type": "STRING",
                      "enum": ["Alimentação", "Transporte", "Moradia", "Saúde", "Lazer", "Outros"]
                    }
                  },
                  "required": [
                    "dataDespesa",
                    "descricaoDespesa",
                    "valorDespesa",
                    "modalidadePagamento",
                    "categoriaDespesa"
                  ]
                }
              },
              "required": ["draft"]
            }
            """);

        return document.RootElement.Clone();
    }

    private sealed record GeminiRequest(
        [property: JsonPropertyName("contents")] IReadOnlyList<GeminiContent> Contents,
        [property: JsonPropertyName("generationConfig")] GeminiGenerationConfig GenerationConfig);

    private sealed record GeminiContent(
        [property: JsonPropertyName("parts")] IReadOnlyList<GeminiPart> Parts);

    private sealed record GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; init; }

        [JsonPropertyName("inline_data")]
        public GeminiInlineData InlineData { get; init; }
    }

    private sealed record GeminiInlineData(
        [property: JsonPropertyName("mime_type")] string MimeType,
        [property: JsonPropertyName("data")] string Data);

    private sealed record GeminiGenerationConfig(
        [property: JsonPropertyName("responseMimeType")] string ResponseMimeType,
        [property: JsonPropertyName("responseSchema")] JsonElement ResponseSchema)
    {
        public GeminiGenerationConfig(JsonElement responseSchema)
            : this("application/json", responseSchema)
        {
        }
    }
}
