using FluentAssertions;
using SorteioEstacionamento.Application;
using SorteioEstacionamento.Domain;

namespace SorteioEstacionamento.Application.Tests;

public sealed class UseCasesCadastroTests
{
    [Fact]
    public async Task CadastrarUnidadePersisteNumeroUnico()
    {
        var unidades = new UnidadeRepositorioEmMemoria();
        var useCase = new CadastrarUnidade(unidades);

        var dto = await useCase.ExecutarAsync(new CadastrarUnidadeCommand(" 101 "));

        dto.Numero.Should().Be("101");
        (await unidades.ListarTodasAsync()).Should().ContainSingle(u => u.Numero == "101");
    }

    [Fact]
    public async Task CadastrarUnidadeDuplicadaFalha()
    {
        var unidades = new UnidadeRepositorioEmMemoria(Unidade.Criar("101"));
        var useCase = new CadastrarUnidade(unidades);

        var act = () => useCase.ExecutarAsync(new CadastrarUnidadeCommand("101"));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroDuplicado);
    }

    [Fact]
    public async Task CadastrarUnidadeSemNumeroFalha()
    {
        var useCase = new CadastrarUnidade(new UnidadeRepositorioEmMemoria());

        var act = () => useCase.ExecutarAsync(new CadastrarUnidadeCommand("  "));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroInvalido);
    }

    [Fact]
    public async Task AtualizarUnidadeAlteraNumero()
    {
        var unidade = Unidade.Criar("101");
        var unidades = new UnidadeRepositorioEmMemoria(unidade);
        var useCase = new AtualizarUnidade(unidades);

        var dto = await useCase.ExecutarAsync(new AtualizarUnidadeCommand(unidade.Id, "102"));

        dto.Numero.Should().Be("102");
        unidade.Numero.Should().Be("102");
    }

    [Fact]
    public async Task AtualizarUnidadeParaNumeroExistenteFalha()
    {
        var a = Unidade.Criar("101");
        var b = Unidade.Criar("102");
        var useCase = new AtualizarUnidade(new UnidadeRepositorioEmMemoria(a, b));

        var act = () => useCase.ExecutarAsync(new AtualizarUnidadeCommand(a.Id, "102"));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroDuplicado);
    }

    [Fact]
    public async Task RemoverUnidadeRemoveDoCadastro()
    {
        var unidade = Unidade.Criar("101");
        var unidades = new UnidadeRepositorioEmMemoria(unidade);
        var useCase = new RemoverUnidade(unidades);

        await useCase.ExecutarAsync(unidade.Id);

        (await unidades.ListarTodasAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task CadastrarVagaGavetaContaDoisLugaresNoInventario()
    {
        var unidades = new UnidadeRepositorioEmMemoria(Unidade.Criar("101"));
        var vagas = new VagaRepositorioEmMemoria();
        var cadastrar = new CadastrarVaga(vagas);
        var inventario = new ObterResumoInventario(unidades, vagas);

        var resultado = await cadastrar.ExecutarAsync(
            new CadastrarVagaCommand("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta, EhPcd: false));
        var resumo = await inventario.ExecutarAsync();

        resultado.Vaga.LugaresFisicos.Should().Be(2);
        resumo.TotalUnidades.Should().Be(1);
        resumo.TotalVagas.Should().Be(1);
        resumo.LugaresFisicos.Should().Be(2);
        resumo.VagasPcd.Should().Be(0);
    }

    [Fact]
    public async Task CadastrarVagaCodigoDuplicadoFalha()
    {
        var vagas = new VagaRepositorioEmMemoria(Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta));
        var useCase = new CadastrarVaga(vagas);

        var act = () => useCase.ExecutarAsync(
            new CadastrarVagaCommand("S1", TipoVaga.Simples, CoberturaVaga.Descoberta, false));

        var ex = await act.Should().ThrowAsync<FalhaRegraNegocioException>();
        ex.Which.Codigo.Should().Be(CodigosErro.CadastroDuplicado);
    }

    [Fact]
    public async Task CadastrarTerceiraVagaPcdGeraAvisoSemBloquear()
    {
        var vagas = new VagaRepositorioEmMemoria(
            Vaga.Criar("P1", TipoVaga.Simples, CoberturaVaga.Descoberta, ehPcd: true),
            Vaga.Criar("P2", TipoVaga.Simples, CoberturaVaga.Descoberta, ehPcd: true));
        var useCase = new CadastrarVaga(vagas);

        var resultado = await useCase.ExecutarAsync(
            new CadastrarVagaCommand("P3", TipoVaga.Simples, CoberturaVaga.Coberta, EhPcd: true));

        resultado.Vaga.Codigo.Should().Be("P3");
        resultado.Avisos.Should().ContainSingle(a => a.Codigo == CodigosAviso.PcdAcimaDoReferencial);
        (await vagas.ListarTodasAsync()).Should().HaveCount(3);
    }

    [Fact]
    public async Task ResumoInventarioSomaLugaresFisicos()
    {
        var unidades = new UnidadeRepositorioEmMemoria(Unidade.Criar("101"), Unidade.Criar("102"));
        var vagas = new VagaRepositorioEmMemoria(
            Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta),
            Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta, ehPcd: true),
            Vaga.Criar("S2", TipoVaga.Simples, CoberturaVaga.Descoberta));
        var inventario = new ObterResumoInventario(unidades, vagas);

        var resumo = await inventario.ExecutarAsync();

        resumo.Should().Be(new ResumoInventarioDto(2, 3, 4, 1));
    }

    [Fact]
    public async Task RemoverVagaAtualizaInventario()
    {
        var gaveta = Vaga.Criar("G1", TipoVaga.Gaveta, CoberturaVaga.Coberta);
        var vagas = new VagaRepositorioEmMemoria(gaveta, Vaga.Criar("S1", TipoVaga.Simples, CoberturaVaga.Coberta));
        var inventario = new ObterResumoInventario(new UnidadeRepositorioEmMemoria(), vagas);

        await new RemoverVaga(vagas).ExecutarAsync(gaveta.Id);
        var resumo = await inventario.ExecutarAsync();

        resumo.LugaresFisicos.Should().Be(1);
        resumo.TotalVagas.Should().Be(1);
    }
}
