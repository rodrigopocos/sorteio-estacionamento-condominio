using System.Buffers.Binary;
using System.Security.Cryptography;

namespace SorteioEstacionamento.Domain;

public static class MotorSorteio
{
    public static ResultadoExecucaoSorteio Executar(
        IReadOnlyList<VagaSnapshot> vagasCadastradas,
        IReadOnlyList<UnidadeSorteioContext> unidades,
        byte[] seed)
    {
        ArgumentNullException.ThrowIfNull(vagasCadastradas);
        ArgumentNullException.ThrowIfNull(unidades);
        ArgumentNullException.ThrowIfNull(seed);
        if (seed.Length == 0)
            throw new ArgumentException("Seed não pode ser vazia.", nameof(seed));

        var seedCopia = seed.ToArray();
        var manuais = unidades.SelectMany(u => u.AtribuicoesManuais).ToList();
        var pool = PoolElegivel.Criar(vagasCadastradas, manuais);
        var rng = new GeradorDeterministico(seedCopia);

        var elegiveis = unidades
            .Where(u => u.PendenciaLugares > 0)
            .OrderBy(u => u.UnidadeId, StringComparer.Ordinal)
            .ToList();

        Permutar(elegiveis, rng);

        var atribuicoesSorteio = new Dictionary<string, IReadOnlyList<AtribuicaoSnapshot>>(StringComparer.Ordinal);

        foreach (var unidade in elegiveis)
        {
            var opcoes = pool.ListarOpcoes(unidade);
            if (opcoes.Count == 0)
                continue;

            var escolhida = opcoes[rng.Next(opcoes.Count)];
            pool.Consumir(escolhida);
            atribuicoesSorteio[unidade.UnidadeId] = escolhida.Vagas
                .Select(v => new AtribuicaoSnapshot(unidade.UnidadeId, v, OrigemAtribuicao.Sorteio))
                .ToList();
        }

        var resultados = unidades.Select(unidade =>
        {
            var sorteio = atribuicoesSorteio.GetValueOrDefault(unidade.UnidadeId) ?? [];
            var todas = unidade.AtribuicoesManuais.Concat(sorteio).ToList();
            var status = CalculadorStatusContemplacao.Calcular(todas);
            return new ResultadoSorteioUnidade(unidade.UnidadeId, sorteio, status);
        }).ToList();

        return new ResultadoExecucaoSorteio(
            seedCopia,
            elegiveis.Select(u => u.UnidadeId).ToList(),
            resultados);
    }

    private static void Permutar(IList<UnidadeSorteioContext> itens, GeradorDeterministico rng)
    {
        for (var i = itens.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (itens[i], itens[j]) = (itens[j], itens[i]);
        }
    }

    private sealed class GeradorDeterministico
    {
        private readonly byte[] _seed;
        private readonly byte[] _buffer = new byte[32];
        private ulong _bloco;
        private int _indice = 32;

        public GeradorDeterministico(byte[] seed)
        {
            _seed = seed;
        }

        public int Next(int maximoExclusivo)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximoExclusivo);
            if (maximoExclusivo == 1)
                return 0;

            var faixa = (uint)maximoExclusivo;
            var limite = uint.MaxValue - uint.MaxValue % faixa;
            uint amostra;
            do
            {
                amostra = ProximoUInt32();
            } while (amostra >= limite);

            return (int)(amostra % faixa);
        }

        private uint ProximoUInt32()
        {
            if (_indice > 28)
                Recarregar();

            var valor = BinaryPrimitives.ReadUInt32BigEndian(_buffer.AsSpan(_indice));
            _indice += 4;
            return valor;
        }

        private void Recarregar()
        {
            var entrada = new byte[_seed.Length + sizeof(ulong)];
            Buffer.BlockCopy(_seed, 0, entrada, 0, _seed.Length);
            BinaryPrimitives.WriteUInt64BigEndian(entrada.AsSpan(_seed.Length), _bloco++);
            SHA256.HashData(entrada, _buffer);
            _indice = 0;
        }
    }
}
