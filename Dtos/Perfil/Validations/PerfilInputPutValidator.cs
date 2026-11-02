using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class PerfilInputPutValidator : AbstractValidator<(Guid id, PerfilInputPutDTO input)>
{
    private readonly CrudContext _db;

    public PerfilInputPutValidator(CrudContext db)
    {
        _db = db;

        // 1. Valida se o registro sendo editado realmente existe no banco central
        RuleFor(x => x.id)
            .MustAsync(async (id, cancellation) =>
            {
                return await _db.Perfis.AnyAsync(x => x.Id == id, cancellation);
            })
            .WithErrorCode("NotFound")
            .WithMessage(x => $"Perfil não encontrado.");

        // 2. Valida a duplicidade global do nome
        RuleFor(x => x.input.Nome)
            .NotEmpty().WithMessage("O Nome do Perfil é obrigatório.")
            .Length(3, 50).WithMessage("O Nome do Perfil deve ter entre 3 e 50 caracteres.")
            .MustAsync(async (contexto, nome, cancellation) =>
                !await _db.Perfis.AnyAsync(p => p.Nome == nome && p.Id != contexto.id, cancellation))
            .WithMessage(x => $"O Perfil com o nome '{x.input.Nome}' já está cadastrado.");
    }
}
