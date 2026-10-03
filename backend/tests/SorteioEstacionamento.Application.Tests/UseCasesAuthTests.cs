using FluentAssertions;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application.Tests;

public sealed class UseCasesAuthTests
{
    [Fact]
    public async Task CredenciaisValidasNaoLanca()
    {
        var useCase = new AutenticarAdmin(new AdminCredenciaisFake());

        var act = () => useCase.ExecutarAsync(new LoginCommand("admin", "segredo"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CredenciaisInvalidasLancaAuthInvalidComMensagemGenerica()
    {
        var useCase = new AutenticarAdmin(new AdminCredenciaisFake());

        var usuarioErrado = () => useCase.ExecutarAsync(new LoginCommand("outro", "segredo"));
        var senhaErrada = () => useCase.ExecutarAsync(new LoginCommand("admin", "errada"));

        var exUsuario = await usuarioErrado.Should().ThrowAsync<FalhaRegraNegocioException>();
        var exSenha = await senhaErrada.Should().ThrowAsync<FalhaRegraNegocioException>();
        exUsuario.Which.Codigo.Should().Be(CodigosErro.AuthInvalido);
        exSenha.Which.Codigo.Should().Be(CodigosErro.AuthInvalido);
        exUsuario.Which.Message.Should().Be(exSenha.Which.Message);
        exUsuario.Which.Message.Should().Be("Usuário ou senha inválidos.");
    }
}
