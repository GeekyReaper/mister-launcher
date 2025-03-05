using apiGameDb.Models;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using Microsoft.AspNetCore.Mvc;
using MiSTerLauncher.Server.HostedService;

namespace MiSTerLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private MisterManager _misterManager;

        public SystemController(ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService) : base()
        {
            hostedService.Wakeup();
            _misterManager = hostedService.manager;
        }

        [HttpPost("search")]       
        public async Task<ActionResult<SystemSearchResult>> Search(GameSystemSearch search)
        {
            var result = await _misterManager.GetSystemResult(search);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("id")]        
        public async Task<ActionResult<SystemDb>> Details(BodyIdPost idParameters)
        {
            var result = await _misterManager.GetSystem(idParameters.Id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("updatesettings")]
        public async Task<ActionResult<SystemDb>> UpdateSettings(SystemDb system)
        {
            var result = await _misterManager.UpdateSettingsSystem(system);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("countfilter")]
        public async Task<ActionResult<List<ItemCount>>> CountFilter(BodyCountFilter parameters)
        {
            var result = await _misterManager.GetSystemsFilter(parameters.filter);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("scan")]
        public async Task<ActionResult<bool>> Scan()
        {
            var result = _misterManager.AutomaticScanSystems();
            if (!result)
                return NotFound();
            return Ok(result);
        }


    }
}
