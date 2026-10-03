using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SorteioEstacionamento.Application;

namespace SorteioEstacionamento.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AutenticarAdmin _autenticar;

    public AuthController(AutenticarAdmin autenticar) => _autenticar = autenticar;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand comando, CancellationToken cancellationToken)
    {
        await _autenticar.ExecutarAsync(comando, cancellationToken);
        var identidade = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, comando.Usuario)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade));
        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<SessaoDto> Me() =>
        Ok(new SessaoDto(User.Identity?.Name ?? "admin"));
}

public sealed record SessaoDto(string Usuario);
