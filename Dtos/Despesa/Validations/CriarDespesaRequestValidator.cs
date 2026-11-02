using FluentValidation;

public sealed class CriarDespesaRequestValidator : AbstractValidator<CriarDespesaRequest>
{
    public CriarDespesaRequestValidator()
    {
        RuleFor(x => x.DataDespesa)
            .NotEmpty().WithMessage("A data da despesa é obrigatória.");

        RuleFor(x => x.DescricaoDespesa)
            .NotEmpty().WithMessage("A descrição da despesa é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.");

        RuleFor(x => x.ValorDespesa)
            .GreaterThan(0).WithMessage("O valor da despesa deve ser maior que zero.")
            .LessThanOrEqualTo(999999999.99m).WithMessage("O valor da despesa excede o limite permitido.");

        RuleFor(x => x.ModalidadePagamento)
            .NotEmpty().WithMessage("A modalidade de pagamento é obrigatória.")
            .MaximumLength(100).WithMessage("A modalidade de pagamento não pode exceder 100 caracteres.");

        RuleFor(x => x.CategoriaDespesa)
            .NotEmpty().WithMessage("A categoria da despesa é obrigatória.")
            .MaximumLength(100).WithMessage("A categoria da despesa não pode exceder 100 caracteres.");
    }
}
