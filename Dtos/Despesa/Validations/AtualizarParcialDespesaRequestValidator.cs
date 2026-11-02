using FluentValidation;

public sealed class AtualizarParcialDespesaRequestValidator : AbstractValidator<AtualizarParcialDespesaRequest>
{
    public AtualizarParcialDespesaRequestValidator()
    {
        RuleFor(x => x.DataDespesa)
            .Must(dataDespesa => !dataDespesa.HasValue || dataDespesa.Value != default)
            .WithMessage("A data da despesa informada é inválida.");

        RuleFor(x => x.DescricaoDespesa)
            .NotEmpty().WithMessage("A descrição da despesa não pode ficar vazia.")
            .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.")
            .When(x => x.DescricaoDespesa is not null);

        RuleFor(x => x.ValorDespesa)
            .GreaterThan(0).WithMessage("O valor da despesa deve ser maior que zero.")
            .LessThanOrEqualTo(999999999.99m).WithMessage("O valor da despesa excede o limite permitido.")
            .When(x => x.ValorDespesa.HasValue);

        RuleFor(x => x.ModalidadePagamento)
            .NotEmpty().WithMessage("A modalidade de pagamento não pode ficar vazia.")
            .MaximumLength(100).WithMessage("A modalidade de pagamento não pode exceder 100 caracteres.")
            .When(x => x.ModalidadePagamento is not null);

        RuleFor(x => x.CategoriaDespesa)
            .NotEmpty().WithMessage("A categoria da despesa não pode ficar vazia.")
            .MaximumLength(100).WithMessage("A categoria da despesa não pode exceder 100 caracteres.")
            .When(x => x.CategoriaDespesa is not null);
    }
}
