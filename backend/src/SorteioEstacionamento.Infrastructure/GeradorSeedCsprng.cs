using System.Security.Cryptography;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Infrastructure;

public sealed class GeradorSeedCsprng : IGeradorSeedSorteio
{
    public byte[] Gerar32Bytes()
    {
        var seed = new byte[32];
        RandomNumberGenerator.Fill(seed);
        return seed;
    }
}
