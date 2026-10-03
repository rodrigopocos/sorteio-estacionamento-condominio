using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application;

public sealed class AutenticarAdmin
{
    private readonly IAdminCredenciais _credenciais;

    public AutenticarAdmin(IAdminCredenciais credenciais) => _credenciais = credenciais;

    public Task ExecutarAsync(LoginCommand comando, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_credenciais.Conferir(comando.Usuario ?? "", comando.Senha ?? ""))
            throw new FalhaRegraNegocioException(CodigosErro.AuthInvalido, "Usuário ou senha inválidos.");

        return Task.CompletedTask;
    }
}
