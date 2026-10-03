using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/vagas")]
public sealed class VagasController : ControllerBase
{
    private readonly ConsultarVagas _consultar;
    private readonly CadastrarVaga _cadastrar;
    private readonly AtualizarVaga _atualizar;
    private readonly RemoverVaga _remover;

    public VagasController(
        ConsultarVagas consultar,
        CadastrarVaga cadastrar,
        AtualizarVaga atualizar,
        RemoverVaga remover)
    {
        _consultar = consultar;
        _cadastrar = cadastrar;
        _atualizar = atualizar;
        _remover = remover;
    }

    [HttpGet]
    public Task<IReadOnlyList<VagaDto>> Listar(CancellationToken cancellationToken) =>
        _consultar.ListarAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<VagaDto> Obter(Guid id, CancellationToken cancellationToken) =>
        _consultar.ObterAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<ResultadoVaga>> Cadastrar(
        [FromBody] CadastrarVagaCommand comando,
        CancellationToken cancellationToken)
    {
        var resultado = await _cadastrar.ExecutarAsync(comando, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = resultado.Vaga.Id }, resultado);
    }

    [HttpPut("{id:guid}")]
    public Task<ResultadoVaga> Atualizar(
        Guid id,
        [FromBody] CadastrarVagaCommand comando,
        CancellationToken cancellationToken) =>
        _atualizar.ExecutarAsync(
            new AtualizarVagaCommand(id, comando.Codigo, comando.Tipo, comando.Cobertura, comando.EhPcd),
            cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _remover.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
