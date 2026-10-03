using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/rodadas")]
public sealed class RodadasController : ControllerBase
{
    private readonly CriarRodada _criar;
    private readonly ListarRodadas _listar;
    private readonly RemoverRodada _removerRodada;
    private readonly AtivarRodadaAnual _ativar;
    private readonly ObterDetalheRodada _obter;
    private readonly AtribuirVagaManual _atribuir;
    private readonly RemoverAtribuicaoManual _remover;
    private readonly ExecutarSorteio _executarSorteio;
    private readonly CancelarSorteio _cancelarSorteio;
    private readonly ExportarRodadaCsv _exportarCsv;
    private readonly ExportarRodadaPdf _exportarPdf;

    public RodadasController(
        CriarRodada criar,
        ListarRodadas listar,
        RemoverRodada removerRodada,
        AtivarRodadaAnual ativar,
        ObterDetalheRodada obter,
        AtribuirVagaManual atribuir,
        RemoverAtribuicaoManual remover,
        ExecutarSorteio executarSorteio,
        CancelarSorteio cancelarSorteio,
        ExportarRodadaCsv exportarCsv,
        ExportarRodadaPdf exportarPdf)
    {
        _criar = criar;
        _listar = listar;
        _removerRodada = removerRodada;
        _ativar = ativar;
        _obter = obter;
        _atribuir = atribuir;
        _remover = remover;
        _executarSorteio = executarSorteio;
        _cancelarSorteio = cancelarSorteio;
        _exportarCsv = exportarCsv;
        _exportarPdf = exportarPdf;
    }

    [HttpGet]
    public Task<IReadOnlyList<RodadaResumoDto>> Listar(CancellationToken cancellationToken) =>
        _listar.ExecutarAsync(cancellationToken);

    [HttpPost]
    public async Task<ActionResult<RodadaDetalheDto>> Criar(
        [FromBody] CriarRodadaCommand comando,
        CancellationToken cancellationToken)
    {
        var dto = await _criar.ExecutarAsync(comando, cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id = dto.Id }, dto);
    }

    [HttpPost("{id:guid}/ativar")]
    public async Task<RodadaDetalheDto> Ativar(Guid id, CancellationToken cancellationToken)
    {
        await _ativar.ExecutarAsync(id, cancellationToken);
        return await _obter.ExecutarAsync(id, cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public Task<RodadaDetalheDto> Obter(Guid id, CancellationToken cancellationToken) =>
        _obter.ExecutarAsync(id, cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _removerRodada.ExecutarAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/atribuicoes-manuais")]
    public async Task<ActionResult<AtribuicaoCriadaDto>> AtribuirManual(
        Guid id,
        [FromBody] AtribuirManualRequest request,
        CancellationToken cancellationToken)
    {
        var atribuicaoId = await _atribuir.ExecutarAsync(
            new AtribuirVagaManualCommand(id, request.UnidadeId, request.VagaId),
            cancellationToken);
        return CreatedAtAction(nameof(Obter), new { id }, new AtribuicaoCriadaDto(atribuicaoId));
    }

    [HttpDelete("{id:guid}/atribuicoes-manuais/{atribuicaoId:guid}")]
    public async Task<IActionResult> RemoverManual(
        Guid id,
        Guid atribuicaoId,
        CancellationToken cancellationToken)
    {
        await _remover.ExecutarAsync(new RemoverAtribuicaoManualCommand(id, atribuicaoId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/sorteio/executar")]
    public Task<ResultadoSorteioDto> ExecutarSorteio(Guid id, CancellationToken cancellationToken) =>
        _executarSorteio.ExecutarAsync(id, cancellationToken);

    [HttpPost("{id:guid}/sorteio/cancelar")]
    public async Task<RodadaDetalheDto> CancelarSorteio(Guid id, CancellationToken cancellationToken)
    {
        await _cancelarSorteio.ExecutarAsync(id, cancellationToken);
        return await _obter.ExecutarAsync(id, cancellationToken);
    }

    [HttpGet("{id:guid}/export/csv")]
    public async Task<IActionResult> ExportarCsv(Guid id, CancellationToken cancellationToken)
    {
        var arquivo = await _exportarCsv.ExecutarAsync(id, cancellationToken);
        return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeArquivo);
    }

    [HttpGet("{id:guid}/export/pdf")]
    public async Task<IActionResult> ExportarPdf(Guid id, CancellationToken cancellationToken)
    {
        var arquivo = await _exportarPdf.ExecutarAsync(id, cancellationToken);
        return File(arquivo.Conteudo, arquivo.TipoConteudo, arquivo.NomeArquivo);
    }
}
