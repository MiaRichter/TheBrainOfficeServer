using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Dapper;
using Microsoft.AspNetCore.Identity;
using TheBrainOfficeServer.Services;
using TheBrainOfficeServer.Models;

public class UserLoginService
{
    private readonly AppDbService _db;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserLoginService(AppDbService db, IHttpContextAccessor contextAccessor, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _contextAccessor = contextAccessor;
        _passwordHasher = passwordHasher;
    }

    // 🔹 Регистрация пользователя
    public async Task<bool> RegisterAsync(string username, string password)
    {
        var existingUser = await _db.GetScalarAsync<User>(
            "SELECT * FROM users WHERE username = @username",
            new { username }
        );

        if (existingUser != null)
            return false; // Пользователь уже есть

        var user = new User { Username = username };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        return await _db.ExecuteAsync(
            "INSERT INTO users (username, password_hash) VALUES (@Username, @PasswordHash)",
            user
        );
    }

    // 🔹 Авторизация (вход)
    public async Task<bool> LoginAsync(string username, string password)
    {
        // Ищем пользователя в БД
        var user = await _db.GetScalarAsync<User>(
            "SELECT username, password_hash AS PasswordHash FROM users WHERE username = @username",
            new { username }
        );

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
            return false;

        // Проверяем пароль
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success;
    }

    // 🔹 Проверка текущего пользователя (как в твоём коде)
    public string CurrentName
    {
        get
        {
            var name = _contextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(name))
                throw new UnauthorizedAccessException("Not logged in");
            return name;
        }
    }
    
    
}