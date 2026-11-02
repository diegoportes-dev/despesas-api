using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class BrazilianDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset>
{
    private const string BrazilianDateFormat = "dd/MM/yyyy HH:mm:ss";
    private static readonly TimeSpan BrazilianOffset = TimeSpan.FromHours(-3);
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("A data deve ser uma string no formato dd/MM/yyyy HH:mm:ss.");
        }

        var value = reader.GetString();
        if (DateTime.TryParseExact(value, BrazilianDateFormat, BrazilianCulture, DateTimeStyles.None, out var localDate))
        {
            return new DateTimeOffset(DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified), BrazilianOffset);
        }

        if (value is not null && HasExplicitOffset(value) &&
            DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateWithOffset))
        {
            return dateWithOffset;
        }

        throw new JsonException("A data deve estar no formato dd/MM/yyyy HH:mm:ss ou ISO 8601 com fuso horário.");
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToOffset(BrazilianOffset).ToString(BrazilianDateFormat, CultureInfo.InvariantCulture));
    }

    private static bool HasExplicitOffset(string value) =>
        value.EndsWith('Z') ||
        value.Length >= 6 && (value[^6] is '+' or '-') && value[^3] == ':';
}

public sealed class BrazilianNullableDateTimeOffsetJsonConverter : JsonConverter<DateTimeOffset?>
{
    private static readonly BrazilianDateTimeOffsetJsonConverter Converter = new();

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType == JsonTokenType.Null
            ? null
            : Converter.Read(ref reader, typeof(DateTimeOffset), options);

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            Converter.Write(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
