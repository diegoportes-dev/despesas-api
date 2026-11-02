using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Crud.Data;

public class UsuarioInputPutValidator : AbstractValidator<(Guid id, UsuarioInputPutDto input)>
{
    private readonly CrudContext _db;
    
    public UsuarioInputPutValidator(CrudContext db)
    {
        _db = db;

        // 1. VALIDAÇÃO DE EXISTÊNCIA: Garante que o usuário sendo editado existe
        RuleFor(x => x.id)
            .MustAsync(async (id, cancellation) =>
            {
                return await _db.Usuarios.AnyAsync(u => u.Id == id, cancellation);
            })
            .WithErrorCode("NotFound") 
            .WithMessage(x => $"Usuário não encontrado.");
        
        // 2. VALIDAÇÃO DE E-MAIL
        RuleFor(x => x.input.Email)
            .NotEmpty().WithMessage("O E-Mail do usuário é obrigatório.")
            .EmailAddress().WithMessage("O E-Mail fornecido não é válido.")
            .MaximumLength(100).WithMessage("O E-Mail do usuário não pode exceder 100 caracteres.")  
            .MustAsync(async (model, email, cancellation) =>
                !await _db.Usuarios.AnyAsync(u => u.Email == email && u.Id != model.id, cancellation))
            .WithMessage(x => $"O E-Mail {x.input.Email} já está em uso por outro usuário.");

        // 3. VALIDAÇÃO DE SENHA ALTERNATIVA: Mantém as regras do seu projeto original
        RuleFor(x => x.input.NovaSenha)
            .Length(6, 8).WithMessage("A Nova Senha do usuário deve ter entre 6 e 8 caracteres.")
            .Matches(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{6,8}$")
            .WithMessage("A Nova Senha deve conter pelo menos uma letra maiúscula, uma minúscula e um número.")  
            .When(x => !string.IsNullOrEmpty(x.input.NovaSenha));

        // 4. VALIDAÇÃO DE PERFIL
        RuleFor(x => x.input.PerfilId)
            .NotEmpty().WithMessage("O Perfil é obrigatório.")
            .MustAsync(async (perfilId, cancellation) =>
                await _db.Perfis.AnyAsync(p => p.Id == perfilId, cancellation))
            .WithMessage("Perfil não encontrado.");
    }
}
