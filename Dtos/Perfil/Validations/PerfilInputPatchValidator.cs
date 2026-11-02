using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class PerfilInputPatchValidator : AbstractValidator<(Guid id, PerfilInputPatchDto input)>
{
    private readonly CrudContext _db;

    public PerfilInputPatchValidator(CrudContext db)
    {
        _db = db;

        // 1. Valida se o registro sendo modificado existe no catálogo central
        RuleFor(x => x.id)
            .MustAsync(async (id, cancellation) =>
            {
                return await _db.Perfis.AnyAsync(p => p.Id == id, cancellation);
            })
            .WithErrorCode("NotFound")
            .WithMessage(x => $"Perfil não encontrado.");

        // 2. Valida a duplicidade global do nome
        RuleFor(x => x.input.Nome)
            .Length(3, 50).WithMessage("O Nome do Perfil deve ter entre 3 e 50 caracteres.")
            .MustAsync(async (contexto, nome, cancellation) =>
                !await _db.Perfis.AnyAsync(p => p.Nome == nome && p.Id != contexto.id, cancellation))
            .WithMessage(x => $"O Perfil com o nome '{x.input.Nome}' já está cadastrado.")
            .When(x => !string.IsNullOrEmpty(x.input.Nome)); // Só executa se o nome for enviado no PATCH

        // 3. Valida a formatação de ativação aceitando apenas 'S' ou 'N' de forma flexível
        RuleFor(x => x.input.Ativo)            
            .Matches(@"^[SsNn]$")
            .WithMessage("O status deve ser obrigatoriamente 'S' (Sim) ou 'N' (Não).")
            .When(x => !string.IsNullOrEmpty(x.input.Ativo));
    }
}
