using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class NivelAcessoInputPutDtoValidator : AbstractValidator<(Guid Id, NivelAcessoInputPutDTO Input)>
{
    private readonly CrudContext _db;

    public NivelAcessoInputPutDtoValidator(CrudContext db)
    {
        _db = db;

        // 1. Valida se o registro principal de Nível de Acesso existe na base
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("O ID do nível de acesso é obrigatório.")
            .MustAsync(async (id, cancellationToken) => 
                await _db.NiveisAcessos.AnyAsync(n => n.Id == id, cancellationToken))
            .WithMessage("Nível de acesso não localizado no sistema.")
            .WithErrorCode("NotFound");

        // 2. Regra para o PerfilId informado no Payload (Garante existência)
        RuleFor(x => x.Input.PerfilId)
            .NotEmpty().WithMessage("O ID do Perfil é obrigatório.")
            .MustAsync(async (perfilId, cancellationToken) => 
                await _db.Perfis.AnyAsync(p => p.Id == perfilId, cancellationToken))
            .WithMessage("O Perfil informado para atualização não existe.")
            .WithErrorCode("NotFound");

        // 3. Regra para o RotaId informado no Payload (Garante existência)
        RuleFor(x => x.Input.RotaId)
            .NotEmpty().WithMessage("O ID da Rota é obrigatório.")
            .MustAsync(async (rotaId, cancellationToken) => 
                await _db.Rotas.AnyAsync(r => r.Id == rotaId, cancellationToken))
            .WithMessage("A Rota informada para atualização não existe.")
            .WithErrorCode("NotFound");

        // 4. REGRA DE DUPLICIDADE: Valida a combinação única de Perfil + Rota
        RuleFor(x => x)
            .MustAsync(async (tupla, cancellationToken) =>
                !await _db.NiveisAcessos.AnyAsync(n =>
                    n.Id != tupla.Id
                    && n.PefilId == tupla.Input.PerfilId
                    && n.RotaId == tupla.Input.RotaId,
                    cancellationToken))
            .WithMessage("Este Perfil já possui permissão de acesso cadastrada para a Rota informada.")
            .WithName("CombinacaoPerfilRota");
    }
}
