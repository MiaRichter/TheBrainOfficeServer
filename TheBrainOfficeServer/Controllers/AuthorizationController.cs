using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TheBrainOfficeServer.Models;

[ApiController]
[Route("api/auth")]
public class AuthorizationController : ControllerBase
{
    private readonly UserLoginService _authService;
    private readonly IConfiguration _config;

    public AuthorizationController(UserLoginService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    // Регистрация — без изменений
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthorizationRequest request)
    {
        if (!await _authService.RegisterAsync(request.username, request.password))
            return BadRequest("User already exists");

        return Ok("User registered");
    }

    // Логин — возвращаем JWT токен
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthorizationRequest request)
    {
        if (!await _authService.LoginAsync(request.username, request.password))
            return Unauthorized("Invalid credentials");

        var token = GenerateJwtToken(request.username);
        return Ok(new { token });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSecret = _config["Jwt:Secret"] ?? "YourVerySecretKey1234567890";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
            Expires = DateTime.UtcNow.AddDays(30),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            //Issuer = "", // если используешь
            //Audience = ""
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Проверка токена — если невалиден, авторизация не пройдет (через [Authorize])
    [HttpGet("check")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult CheckAuth()
    {
        var userName = User.Identity?.Name;
        if (userName == null)
            return Unauthorized();

        return Ok(userName);
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT stateless — для logout клиент просто удаляет токен
        return Ok();
    }
}
