using HrCommonApi.Authorization;
using HrCommonApi.Database;
using HrCommonApi.Database.Models;
using HrCommonApi.Enums;
using HrCommonApi.Services.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HrCommonApi.Services;

public class UserService(HrDataContext context, IConfiguration configuration) : CoreService<User>(context), IUserService
{
    private IConfiguration Configuration { get; } = configuration;
    private DbSet<Session> Sessions => Context.Set<Session>();

    public async Task<ServiceResult<User>> Login(string username, string password)
    {
        try
        {
            var account = await ServiceTable.FirstOrDefaultAsync(q => q.Username == username);
            if (account == null)
                return new ServiceResult<User>(ServiceResponse.NotFound, message: "Account not found");

            var passwordManager = new PasswordManager();

            if (!passwordManager.VerifyPassword(account.Password!, password))
                return new ServiceResult<User>(ServiceResponse.NotFound, message: "Invalid password");

            var existingSessions = await GetUserSessions(account.Id);
            if (existingSessions.Response != ServiceResponse.Success)
                return new ServiceResult<User>(existingSessions.Response, message: existingSessions.Message, exception: existingSessions.Exception);

            if (existingSessions.Result == null || !existingSessions.Result.Any())
            {
                var session = await RegisterSession(account); // Attempt to register a session
                if (session == null)
                    return new ServiceResult<User>(ServiceResponse.Exception, message: "Failed to register session");

                // Generate a session and use the expiration date generated to set the session expiration in the database
                var jwt = GenerateJwt(username, account.Role, account.Id, session.Id, out var accessExpiration, out var renewExpiration);
                session = await SetJwtTokenAndExpiration(session.Id, jwt, GenerateRefreshToken(), accessExpiration, renewExpiration);
                if (session == null)
                    return new ServiceResult<User>(ServiceResponse.Exception, message: "Failed to set session");
            }

            return new ServiceResult<User>(ServiceResponse.Success, account, message: "Successfully authorized");
        }
        catch (Exception exception)
        {
            return new ServiceResult<User>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    public async Task<ServiceResult<List<Session>>> GetUserSessions(Guid userId)
    {
        try
        {
            if (!ServiceTable.Any(u => u.Id == userId))
                return new ServiceResult<List<Session>>(ServiceResponse.NotFound, message: "User not found");

            var now = DateTime.Now.ToUniversalTime();
            var sessions = await Sessions.Where(q => q.UserId == userId && q.AccessExpiresAt >= now).ToListAsync();
            var message = sessions.Count == 0 ? "No active sessions found" : $"Found {sessions.Count} active sessions";

            return new ServiceResult<List<Session>>(ServiceResponse.Success, sessions, message: message);
        }
        catch (Exception exception)
        {
            return new ServiceResult<List<Session>>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }

    /// <summary>
    /// Generates a JWT token for the user. Also sets the expiration date for the token and the refresh token.
    /// </summary>
    private string GenerateJwt(string username, Role role, Guid id, Guid jti, out DateTime accessExpiration, out DateTime renewExpiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        accessExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(Configuration["Jwt:ExpireMinutes"]));
        renewExpiration = DateTime.Now.AddDays(Convert.ToInt32(Configuration["Jwt:RefreshTokenValidityInDays"]));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, ((int)role).ToString()),
            new Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, jti.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Configuration["Jwt:Issuer"],
            audience: Configuration["Jwt:Audience"],
            claims: claims,
            expires: accessExpiration,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<Session> RegisterSession(User user)
    {
        var now = DateTime.Now.ToUniversalTime();

        var result = await Sessions.AddAsync(new Session()
        {
            UserId = user.Id,
            CreatedAt = now,
            UpdatedAt = now
        });

        await Context.SaveChangesAsync();

        return result.Entity;
    }

    private async Task<Session?> SetJwtTokenAndExpiration(Guid sessionId, string jwt, string renewToken, DateTime accessExpiration, DateTime renewExpiration)
    {
        var token = await Sessions.FindAsync(sessionId);
        if (token == null)
            return null;

        token.AccessToken = jwt;
        token.AccessExpiresAt = accessExpiration.ToUniversalTime();
        token.RefreshToken = renewToken;
        token.RefreshExpiresAt = renewExpiration.ToUniversalTime();
        await Context.SaveChangesAsync();

        return token;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public override async Task<ServiceResult<User>> Create(User entity)
    {
        try
        {
            if (ServiceTable.Any(q => q.Username == entity.Username))
                return new ServiceResult<User>(ServiceResponse.BadRequest, message: "Username already exists!");

            return await base.Create(entity);
        }
        catch (Exception exception)
        {
            return new ServiceResult<User>(ServiceResponse.Exception, exception: exception, message: exception.Message);
        }
    }
}