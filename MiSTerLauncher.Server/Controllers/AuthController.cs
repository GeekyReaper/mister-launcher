using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using apiGameDb.Models;
using libMisterLauncher.Manager;
using MiSTerLauncher.Server.HostedService;
using libMisterLauncher.Entity;
using MongoDB.Driver.Linq;
using System.Diagnostics.Eventing.Reader;

namespace MiSTerLauncher.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _config;
        private MisterManager _misterManager;

        public AuthController(IConfiguration config, MisterBackgroundTask hostedService)
        {
            _config = config;
            hostedService.Wakeup();
            _misterManager = hostedService.manager;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginModel loginmodel)
        {
            ActionResult response = Unauthorized();
              

            if (_misterManager.AuthorisationIsrequired() && _misterManager.CheckAdminPassword(loginmodel.password))
            {
                var claims = new[] {
                        new Claim( ClaimTypes.Role, "admin")
                };
                var tokenString = GenerateJSONWebToken(claims);
                response = Ok(new { token = tokenString, role = "admin" });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpGet("loginwithoutauthentication")]
        public async Task<ActionResult> LoginWithoutAuthentication()
        {
            ActionResult response = Unauthorized();


            if (!_misterManager.AuthorisationIsrequired())
            {
                var claims = new[] {
                        new Claim( ClaimTypes.Role, "admin")
                };
                var tokenString = GenerateJSONWebToken(claims);
                response = Ok(new { token = tokenString, role = "admin" });
            }

            return response;
        }

        [AllowAnonymous]
        [HttpPost("requestguestaccess")]
        public async Task<ActionResult<GuestAccess>> RequestGuestAccess(GuestAccessParams parameters)
        {
            if (!_misterManager.AuthorisationIsrequired())
            {
                return BadRequest();
            }

            var request = _misterManager.GenerateGuessAccess(parameters.signature);
            if (request== null)
            {
                return NotFound();
            }

            return Ok(request);            
        }

        [AllowAnonymous]
        [HttpPost("guestaccessstate")]
        public async Task<ActionResult<GuestAccessState>> GuestAccessState(GuestAccessCodeParams parameters)
        {
            if (!_misterManager.AuthorisationIsrequired())
            {
                return BadRequest();
            }

            return Ok(_misterManager.GuestAccessState(parameters.code));           
        }

        [AllowAnonymous]
        [HttpPost("guestaccessconsumed")]
        public async Task<ActionResult> GuestAccessConsume(GuestAccessConsumeParams parameters)
        {
            

            if (_misterManager.AuthorisationIsrequired() && _misterManager.ConsumedGuestAccess(parameters.code, parameters.key))
            {
                var claims = new[] {
                        new Claim( ClaimTypes.Role, "guest")
                };
                var tokenString = GenerateJSONWebToken(claims);
                return Ok(new { token = tokenString, role = "guest" });
            }

            return Unauthorized();
        }

        [Authorize(Roles ="admin")]
        [HttpGet("guestaccesscurrent")]
        public async Task<ActionResult<List<GuestAccess>>> GuestAccessCurrent()
        {
            return Ok(_misterManager.GetCurrentGuestAccess());
        }

        [Authorize(Roles = "admin")]
        [HttpPost("guestaccessaction")]
        public async Task<ActionResult<bool>> GuestAccessAction(GuestAccessActionParams parameters)
        {
            return Ok(_misterManager.ApprouvedGuestAccess(parameters.approuved, parameters.code));
        }



        private string GenerateJSONWebToken(IEnumerable<Claim>? claims)
        {
            if (claims == null)
                return "";
            var role = TokenType.GUEST;
            foreach( var c in claims)
            {
                if (c.ValueType== ClaimTypes.Role)
                {
                    switch (c.Value)
                    {
                        case "admin":
                            role = TokenType.ADMIN;
                            break;
                        case "guest":
                            role = TokenType.GUEST;
                            break;;
                    }
                }
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var t = new SecurityTokenDescriptor
            {
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                Expires = DateTime.Now.AddMinutes(_misterManager.GetTokenDelay(role)),
                SigningCredentials = credentials,
                Subject = new ClaimsIdentity(claims)
            };

            //var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            //  _config["Jwt:Issuer"],
            //  null,
            //  expires: DateTime.Now.AddMinutes(120),
            //  signingCredentials: credentials);

            var tokenhandler = new JwtSecurityTokenHandler();
            return tokenhandler.WriteToken(tokenhandler.CreateToken(t));

            //return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
