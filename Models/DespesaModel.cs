public class DespesaModel : AuditoriaModel
{
    public Guid Id { get; set; }
    public DateTimeOffset DataDespesa { get; set; }
    public string DescricaoDespesa { get; set; } = string.Empty;
    public decimal ValorDespesa { get; set; }
    public string ModalidadePagamento { get; set; } = string.Empty;
    public string CategoriaDespesa { get; set; } = string.Empty;
}
