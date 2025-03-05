using apiGameDb.Models;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiSTerLauncher.Server.HostedService;

namespace MiSTerLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RomController : ControllerBase
    {
        private MisterManager _misterManager;

        public RomController (ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService) : base()
        {
            hostedService.Wakeup();
            _misterManager = hostedService.manager;
        }


        [HttpPost("unmatchroms")]
        public async Task<ActionResult<List<Rom>>> UnMatchRoms(BodyUnmatchRom parameters)
        {
            
            var result = await _misterManager.GetUnmatchRoms(parameters.category, parameters.systemid);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("id")]
        public async Task<ActionResult<Rom>> Details(BodyIdPost parameters)
        {

            var result = await _misterManager.GetRom(parameters.Id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("linkromtoscrappervideogame")]
        public async Task<ActionResult<Boolean>> LinkRomToScrapperVideogame (BodyLinkRomToScrapperVideogame parameters)
        {
            if (string.IsNullOrEmpty(parameters.romid) || parameters.scrapperVideogame <= 0)
            {
                return NotFound();
            }

            var result = await _misterManager.LinkRomToVideogameId(parameters.romid, parameters.scrapperVideogame, parameters.childroms);            
            return Ok(result);
        }

        [HttpPost("launchjobscanrom")]
        public async Task<ActionResult<Boolean>> LaunchJobScanRom(BodyAutomaticMatchRom parameters)
        {
            if (string.IsNullOrEmpty(parameters.systemid))
            {
                return NotFound();
            }

            var result = _misterManager.AutomaticScanRom(parameters.systemid);
            return Ok(result);
        }

        [HttpPost("delete")]
        public async Task<ActionResult<bool>> Delete(BodyDeleteRom parameters)
        {
            var result = await _misterManager.DeleteRom(parameters.Id, parameters.deletefile);
            return result ? Ok(result) : NotFound();
        }
    }
}
