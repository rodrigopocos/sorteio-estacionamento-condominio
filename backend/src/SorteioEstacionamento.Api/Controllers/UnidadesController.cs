using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/unidades")]
public sealed class UnidadesController : ControllerBase
{
    private readonly ConsultarUnidades _consultar;
    private readonly CadastrarUnidade _cadastrar;
    private readonly AtualizarUnidade _atualizar;
    private readonly RemoverUnidade _remover;

    public UnidadesController(
        ConsultarUnidades consultar,
        CadastrarUnidade cadastrar,
        AtualizarUnidade atualizar,
        RemoverUnidade remover)
    {
        _consultar = consultar;
        _cadastrar = cadastrar;
        _atualizar = atualizar;
        _remover = remover;
    }

    [HttpGet]
    public Task<IReadOnlyList<UnidadeDto>> Listar(CancellationToken cancellationToken) =>
        _consultar.ListarAsync(cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<UnidadeDto> Obter(Guid id, CancellationToken cancellationToken) =>
        _consultar.ObterAsync(id, cancellationToken);

    [HttpPost]
    public async Task<ActionResult<UnidadeDto>> Cadastrar(
        [FromBody] CadastrarUnidadeCommand comando,
        CancellationToken cancellationToken)
    {
        var dto = await _cadastrar.ExecutarAsync(comando, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}")]
    public Task<UnidadeDto> Atualizar(
        Guid id,
        [FromBody] CadastrarUnidadeCommand comando,
        CancellationToken cancellationToken) =>
        _atualizar.ExecutarAsync(new AtualizarUnidadeCommand(id, comando.Numero), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _remover.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }
}
