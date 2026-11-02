using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class NivelAcessoInputPostDtoValidator : AbstractValidator<NivelAcessoInputPostDTO>
{
    private readonly CrudContext _db;

    public NivelAcessoInputPostDtoValidator(CrudContext db)
    {
        _db = db;

        // 1. Regra para o PerfilId (Garante existência)
        RuleFor(x => x.PerfilId)
            .NotEmpty().WithMessage("O ID do Perfil é obrigatório.")
            .MustAsync(async (perfilId, cancellationToken) => 
                await _db.Perfis.AnyAsync(p => p.Id == perfilId, cancellationToken))
            .WithMessage("O Perfil informado não existe no sistema.")
            .WithErrorCode("NotFound");

        // 2. Regra para o RotaId (Garante existência)
        RuleFor(x => x.RotaId)
            .NotEmpty().WithMessage("O ID da Rota é obrigatório.")
            .MustAsync(async (rotaId, cancellationToken) => 
                await _db.Rotas.AnyAsync(r => r.Id == rotaId, cancellationToken))
            .WithMessage("A Rota informada não existe no sistema.")
            .WithErrorCode("NotFound");

        // 3. REGRA DE DUPLICIDADE: Valida a combinação única de Perfil + Rota
        RuleFor(x => x)
            .MustAsync(async (dto, cancellationToken) =>
                !await _db.NiveisAcessos.AnyAsync(n =>
                    n.PefilId == dto.PerfilId && n.RotaId == dto.RotaId,
                    cancellationToken))
            .WithMessage("Este Perfil já possui permissão de acesso cadastrada para a Rota informada.")
            .WithName("CombinacaoPerfilRota");
    }
}
