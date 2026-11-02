using FluentValidation;

public sealed class AtualizarDespesaRequestValidator : AbstractValidator<(Guid Id, AtualizarDespesaRequest Input)>
{
    public AtualizarDespesaRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O identificador da despesa é obrigatório.");

        RuleFor(x => x.Input.DataDespesa)
            .NotEmpty().WithMessage("A data da despesa é obrigatória.");

        RuleFor(x => x.Input.DescricaoDespesa)
            .NotEmpty().WithMessage("A descrição da despesa é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.");

        RuleFor(x => x.Input.ValorDespesa)
            .GreaterThan(0).WithMessage("O valor da despesa deve ser maior que zero.")
            .LessThanOrEqualTo(999999999.99m).WithMessage("O valor da despesa excede o limite permitido.");

        RuleFor(x => x.Input.ModalidadePagamento)
            .NotEmpty().WithMessage("A modalidade de pagamento é obrigatória.")
            .MaximumLength(100).WithMessage("A modalidade de pagamento não pode exceder 100 caracteres.");

        RuleFor(x => x.Input.CategoriaDespesa)
            .NotEmpty().WithMessage("A categoria da despesa é obrigatória.")
            .MaximumLength(100).WithMessage("A categoria da despesa não pode exceder 100 caracteres.");
    }
}
