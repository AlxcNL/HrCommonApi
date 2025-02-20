﻿using HrCommonApi.Authorization;
using HrCommonApi.Database.Models;
using HrCommonApi.Enums;
using HrCommonApi.Services.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HrCommonApi.Services;

public class UserService<TUser, TDataContext>(TDataContext context, IConfiguration configuration) : CoreService<TUser, TDataContext>(context), IUserService<TUser>
    where TUser : User
    where TDataContext : DbContext
{
    private IConfiguration Configuration { get; } = configuration;
    private DbSet<Session> Sessions => Context.Set<Session>();

    public async Task<ServiceResponse<TUser>> Login(string username, string password)
    {
        try
        {
            var account = await ServiceTable.FirstOrDefaultAsync(q => q.Username == username);
            if (account == null)
                return new ServiceResponse<TUser>(ServiceCode.NotFound, message: "Account not found");

            var passwordManager = new PasswordManager();

            if (!passwordManager.VerifyPassword(account.Password!, password))
                return new ServiceResponse<TUser>(ServiceCode.NotFound, message: "Invalid password");

            var sessions = await GetActiveSessions(account.Id);
            if (sessions.Count == 0)
            {
                var session = await RegisterSession(account); // Attempt to register a session
                if (session == null)
                    return new ServiceResponse<TUser>(ServiceCode.Exception, message: "Failed to register session");

                // Generate a session and use the expiration date generated to set the session expiration in the database
                var jwt = GenerateJwt(username, account.Role, account.Id, session.Id, out var accessExpiration, out var renewExpiration);
                session = await SetJwtTokenAndExpiration(session.Id, jwt, GenerateRefreshToken(), accessExpiration, renewExpiration);
                if (session == null)
                    return new ServiceResponse<TUser>(ServiceCode.Exception, message: "Failed to set session");
            }

            return new ServiceResponse<TUser>(ServiceCode.Success, account, message: "Successfully authorized");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TUser>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public async Task<ServiceResponse<List<Session>>> GetUserSessions(Guid userId)
    {
        try
        {
            if (!ServiceTable.Any(u => u.Id == userId))
                return new ServiceResponse<List<Session>>(ServiceCode.NotFound, message: "User not found");

            var sessions = await GetActiveSessions(userId);
            var message = sessions.Count == 0 ? "No active sessions found" : $"Found {sessions.Count} active sessions";

            return new ServiceResponse<List<Session>>(ServiceCode.Success, sessions, message: message);
        }
        catch (Exception exception)
        {
            return new ServiceResponse<List<Session>>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public async Task<List<Session>> GetActiveSessions(Guid userId) =>
        await Sessions.Where(q => q.UserId == userId && q.AccessExpiresAt >= DateTime.Now.ToUniversalTime()).ToListAsync();

    /// <summary>
    /// Generates a JWT token for the user. Also sets the expiration date for the token and the refresh token.
    /// </summary>
    private string GenerateJwt(string username, Role role, Guid id, Guid jti, out DateTime accessExpiration, out DateTime renewExpiration)
    {
        var issuer = Configuration?["HrCommonApi:JwtAuthorization:Jwt:Issuer"];
        var audience = Configuration?["HrCommonApi:JwtAuthorization:Jwt:Audience"];
        var jwtKey = Configuration?["HrCommonApi:JwtAuthorization:Jwt:Key"];

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        accessExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(Configuration?["HrCommonApi:JwtAuthorization:Jwt:TokenExpirationMinutes"]));
        renewExpiration = DateTime.Now.AddMinutes(Convert.ToDouble(Configuration?["HrCommonApi:JwtAuthorization:Jwt:RefreshExpirationInMinutes"]));

        var claims = new[]
        {
            new System.Security.Claims.Claim(ClaimTypes.Name, username),
            new System.Security.Claims.Claim(ClaimTypes.Role, ((int)role).ToString()),
            new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, id.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, jti.ToString())
        };
        var claimsIdentity = new ClaimsIdentity(claims);

        return new JwtSecurityTokenHandler().CreateEncodedJwt(
            issuer: issuer,
            audience: audience,
            subject: claimsIdentity,
            expires: accessExpiration,
            notBefore: DateTime.Now,
            issuedAt: DateTime.Now,
            signingCredentials: creds);
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

    public override async Task<ServiceResponse<TUser>> Create(TUser entity)
    {
        try
        {
            if (ServiceTable.Any(q => q.Username == entity.Username))
                return new ServiceResponse<TUser>(ServiceCode.BadRequest, message: "Username already exists!");

            entity.Password = new PasswordManager().HashPassword(entity.Password!);

            return await base.Create(entity);
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TUser>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }

    public virtual async Task<ServiceResponse<TUser>> UpdateRole(Guid userId, Role role)
    {
        try
        {
            var user = await ServiceTable.FindAsync(userId);

            if (user == null)
                return new ServiceResponse<TUser>(ServiceCode.NotFound, message: "User not found");

            user.Role = role;
            await Context.SaveChangesAsync();

            return new ServiceResponse<TUser>(ServiceCode.Success, user, message: "Role updated for user");
        }
        catch (Exception exception)
        {
            return new ServiceResponse<TUser>(ServiceCode.Exception, exception: exception, message: exception.Message);
        }
    }
}