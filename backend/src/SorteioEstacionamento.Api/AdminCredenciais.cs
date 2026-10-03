using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api;

public sealed class AdminOptions
{
    public string Username { get; set; } = "admin";
    public string? Password { get; set; }
    public string? PasswordHash { get; set; }
}

public sealed class AdminCredenciais : IAdminCredenciais
{
    private readonly PasswordHasher<object> _hasher = new();
    private readonly string _usuario;
    private readonly string _passwordHash;

    public AdminCredenciais(IConfiguration config, IOptions<AdminOptions> options)
    {
        var o = options.Value;
        _usuario = PrimeiroNaoVazio(config["ADMIN_USERNAME"], o.Username) ?? "admin";
        var hash = PrimeiroNaoVazio(config["ADMIN_PASSWORD_HASH"], o.PasswordHash);
        var senha = PrimeiroNaoVazio(config["ADMIN_PASSWORD"], o.Password) ?? "admin";
        _passwordHash = hash ?? _hasher.HashPassword(null!, senha);
    }

    public bool Conferir(string usuario, string senha)
    {
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(senha))
            return false;

        if (!CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(usuario)),
                SHA256.HashData(Encoding.UTF8.GetBytes(_usuario))))
            return false;

        var resultado = _hasher.VerifyHashedPassword(null!, _passwordHash, senha);
        return resultado is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static string? PrimeiroNaoVazio(params string?[] valores) =>
        valores.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
