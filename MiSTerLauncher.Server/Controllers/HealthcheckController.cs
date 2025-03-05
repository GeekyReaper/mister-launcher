using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiSTerLauncher.Server.HostedService;

namespace MiSTerLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthcheckController : Controller
    {
        MisterManager _manager;
        public HealthcheckController(ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService)
        {
            hostedService.Wakeup();
            _manager = hostedService.manager;
            
        }

        [HttpGet("")]
        [AllowAnonymous]
        public async Task<ActionResult<MisterManagerCache>> Get()
        {
            var result = await Task.Run(_manager.Cache);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
