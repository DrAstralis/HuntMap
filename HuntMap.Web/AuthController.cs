using System;
using System.Threading.Tasks;
using HuntMap.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HuntMap.Web;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly SignInManager<ApplicationUser> _sm;

    public AuthController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    {
        _um = um; _sm = sm;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var user = new ApplicationUser { UserName = req.Email, Email = req.Email };
        var result = await _um.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);
        await _sm.SignInAsync(user, isPersistent: true);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _sm.PasswordSignInAsync(req.Email, req.Password, true, lockoutOnFailure: false);
        if (!result.Succeeded) return Unauthorized();
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _sm.SignOutAsync();
        return Ok();
    }
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);