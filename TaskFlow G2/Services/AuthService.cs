namespace TaskFlow_G2.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Cloud.Firestore;
using Microsoft.IdentityModel.Tokens;
using TaskFlow_G2.DTOs;
using TaskFlow_G2.Models;

public class AuthService
{
    private const string UsersCollection = "users";

    private readonly FirebaseService _firebaseService;
    private readonly IConfiguration _configuration;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
        _firebaseService = firebaseService;
        _configuration = configuration;
    }
    
    public async Task<AuthResponseDto?> Register(RegisterRequestDto dto)
    {
        var collection = _firebaseService.GetCollection(UsersCollection);

        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (existing.Count > 0)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = now
        };
        
        await collection.Document(user.Id).SetAsync(new Dictionary<string, object>
        {
            { "Id", user.Id },
            { "FullName", user.FullName },
            { "Email", user.Email },
            { "PasswordHash", user.PasswordHash },
            { "CreatedAt", Timestamp.FromDateTime(now) }
        });

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public async Task<AuthResponseDto?> Login(LoginRequestDto dto)
    {
        var collection = _firebaseService.GetCollection(UsersCollection);

        var snapshot = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
        {
            return null;
        }

        var doc = snapshot.Documents[0];

        var data = doc.ToDictionary();

        var user = new AppUser
        {
            Id = data["Id"].ToString()!,
            FullName = data["FullName"].ToString()!,
            Email = data["Email"].ToString()!,
            PasswordHash = data["PasswordHash"].ToString()!,
            CreatedAt = ((Timestamp)data["CreatedAt"]).ToDateTime()
        };

        if (!VerifyPasswordHash(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            FullName = user.FullName,
            Email = user.Email
        };
    }

    private string GenerateToken(AppUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPasswordHash(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}