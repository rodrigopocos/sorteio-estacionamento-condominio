using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/inventario")]
public sealed class InventarioController : ControllerBase
{
    private readonly ObterResumoInventario _resumo;

    public InventarioController(ObterResumoInventario resumo) => _resumo = resumo;

    [HttpGet("resumo")]
    public Task<ResumoInventarioDto> Resumo(CancellationToken cancellationToken) =>
        _resumo.ExecutarAsync(cancellationToken);
}
