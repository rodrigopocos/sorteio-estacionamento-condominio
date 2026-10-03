using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Api;

public sealed class FalhaRegraNegocioMiddleware
{
    private readonly RequestDelegate _next;

    public FalhaRegraNegocioMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (FalhaRegraNegocioException ex)
        {
            context.Response.StatusCode = StatusDe(ex.Codigo);
            await context.Response.WriteAsJsonAsync(new { codigo = ex.Codigo, mensagem = ex.Message });
        }
    }

    private static int StatusDe(string codigo) => codigo switch
    {
        CodigosErro.AuthInvalido or CodigosErro.AuthRequired => StatusCodes.Status401Unauthorized,
        CodigosErro.NaoEncontrado => StatusCodes.Status404NotFound,
        CodigosErro.CadastroInvalido => StatusCodes.Status400BadRequest,
        CodigosErro.CadastroDuplicado or CodigosErro.CadastroEmUso
            or CodigosErro.RodadaEstadoInvalido or CodigosErro.VagaIndisponivel => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status422UnprocessableEntity
    };
}
