using apiGameDb.Models;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MiSTerLauncher.Server.HostedService;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Text;

namespace MiSTerLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private MisterManager _misterManager;
        private readonly IConfiguration Configuration;


        public MediaController(ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService, IConfiguration configuration) : base()
        {

            hostedService.Wakeup();
            _misterManager = hostedService.manager;
            Configuration = configuration;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<VideoGameDb>> Index(string id, string token)
        {

            if (string.IsNullOrEmpty(id))
                return NotFound(id);

            if (!TokenIsValid(token))
            {
                return Unauthorized();
            }
            var result = await _misterManager.GetMedia(id);
            if (!result.isExists)
                return NotFound(id);

            if (result.contentType.StartsWith("application/force-download") && result.contentType.Contains(".pdf"))
            {
                Response.Headers.Add("Content-Disposition", string.Format("attachment; filename={0}", result.filename));
                return new FileContentResult(result.content, "application/pdf");
            }
            else
            {
                return new FileContentResult(result.content, result.contentType);
            }
           
        }


        private bool TokenIsValid (string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;
            var param = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Configuration["Jwt:Issuer"],
                ValidAudience = Configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
            };
            try
            {
                var claim = new JwtSecurityTokenHandler().ValidateToken(token, param, out _);
                return claim != null;
            }
            catch (Exception)
            {
                return false;
            }

            
        }
       
    }
}
