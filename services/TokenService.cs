using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace API.services
{
    public class TokenService : ITokenService
    {
        //one key for encryupt and decryupt private key and public
        private readonly SymmetricSecurityKey _Key;
        private readonly UserManager<AppUser> _UserManager;

        public TokenService(IConfiguration config, UserManager<AppUser> UserManager)
        {
            _UserManager = UserManager;
            _Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));

        }
        public async Task<string> CreateToken(AppUser user)
        {

            //cred
            var Creds = new SigningCredentials(_Key, SecurityAlgorithms.HmacSha512Signature);

            //claims
            var Claims = new List<Claim>{
             new Claim(JwtRegisteredClaimNames.NameId,user.Id.ToString()),
             new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName)
           };

            var Roles = await _UserManager.GetRolesAsync(user);

            //add the roles to the claim list
            Claims.AddRange(Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            //token itself Descriptor ==> Subject claims , expires, signing           credentails
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(Claims),
                Expires = DateTime.Now.AddHours(24),
                SigningCredentials = Creds
            };
            //tokenhandler ==> create and return
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}