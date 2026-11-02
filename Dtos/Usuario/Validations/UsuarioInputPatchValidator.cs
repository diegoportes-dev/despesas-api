using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class UsuarioInputPatchValidator : AbstractValidator<(Guid Id, UsuarioInputPatchDto Input)>
{
    private readonly CrudContext _db;

    public UsuarioInputPatchValidator(CrudContext db)
    {
        _db = db;

        // 1. VALIDAÇÃO DE EXISTÊNCIA: Garante que o usuário sendo alterado existe no catálogo
        RuleFor(x => x.Id)
            .MustAsync(async (id, cancellation) =>
            {
                return await _db.Usuarios.AnyAsync(u => u.Id == id, cancellation);
            })
            .WithErrorCode("NotFound")
            .WithMessage(x => $"Usuário não encontrado.");

        // 2. VALIDAÇÃO DE E-MAIL PARCIAL
        RuleFor(x => x.Input.Email)
            .EmailAddress().WithMessage("O E-Mail fornecido não é válido.")
            .MaximumLength(100).WithMessage("O E-Mail do usuário não pode exceder 100 caracteres.")
            .MustAsync(async (model, email, cancellation) =>
                !await _db.Usuarios.AnyAsync(u => u.Email == email && u.Id != model.Id, cancellation))
            .WithMessage(x => $"O E-Mail {x.Input.Email} já está em uso por outro usuário.")
            .When(x => !string.IsNullOrEmpty(x.Input.Email));

        // 3. VALIDAÇÃO DE SENHA PARCIAL: Mantém as regras de complexidade originais do seu projeto
        RuleFor(x => x.Input.NovaSenha)
            .Length(6, 8).WithMessage("A Senha do usuário deve ter entre 6 e 8 caracteres.")
            .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,8}$")
            .WithMessage("A senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número.")
            .When(x => !string.IsNullOrEmpty(x.Input.NovaSenha)); 
            
        // 4. VALIDAÇÃO DE PERFIL PARCIAL
        RuleFor(x => x.Input.PerfilId)
            .MustAsync(async (perfilId, cancellation) =>
                await _db.Perfis.AnyAsync(p => p.Id == perfilId, cancellation))
            .WithErrorCode("PerfilNotFound")
            .WithMessage("Perfil não encontrado.")
            .When(x => x.Input.PerfilId != Guid.Empty);
    }
}
