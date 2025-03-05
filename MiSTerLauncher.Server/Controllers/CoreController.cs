using apiGameDb.Models;
using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MiSTerLauncher.Server.HostedService;

namespace MiSTerLauncher.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoreController : ControllerBase
    {
        private MisterManager _misterManager;
        private MisterBackgroundTask _hostedservice;

        public CoreController(ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService) : base()
        {
            _hostedservice = hostedService;
            hostedService.Wakeup();
            _misterManager = hostedService.manager;
        }

        [HttpPost("keyboardcmd")]
        public async Task<ActionResult<bool>> Keyboardcmd(BodyKeyboardCmdPost parameters)
        {
            var result = await _misterManager.SendKeyboardCmd(parameters.cmds, parameters.raw, parameters.delay);
            if (!result)
                return NotFound();
            return Ok(result);
        }
       

        [HttpPost("getallsavetates")]
        public async Task<ActionResult<List<CoreSaveState>>> GetAllSavestates(BodyGetSavestate parameters)
        {
            if (string.IsNullOrEmpty(parameters.romid) || string.IsNullOrEmpty(parameters.videogameid))
                return NotFound(false);
            var result = await _misterManager.GetSaveAllState(parameters.videogameid, parameters.romid);
           
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("savestatecmdsave")]
        public async Task<ActionResult<List<CoreSaveState>>> SavestateCmdSave(BodySavestateCmd parameters)
        {
            if (string.IsNullOrEmpty(parameters.romid) || string.IsNullOrEmpty(parameters.videogameid) || parameters.slot<1 || parameters.slot >4)
                return NotFound(false);
            var result = await _misterManager.SaveStateCmdSave(parameters.videogameid, parameters.romid, parameters.slot);

            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("savestatecmdload")]
        public async Task<ActionResult<List<CoreSaveState>>> SavestateCmdLoad(BodySavestateCmd parameters)
        {
            if (string.IsNullOrEmpty(parameters.romid) || string.IsNullOrEmpty(parameters.videogameid) || parameters.slot < 1 || parameters.slot > 4)
                return NotFound(false);
            var result = await _misterManager.SaveStateCmdLoad(parameters.videogameid, parameters.romid, parameters.slot);

            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("getmodulesettings")]        
        public async Task<ActionResult<List<ModuleSetting>>> GetModuleSettings(BodyGetModuleSettings parameters)
        {
            if (string.IsNullOrEmpty(parameters.modulename))
                return NotFound(false);
            var result = await _misterManager.GetModuleSettings(parameters.modulename);

            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("setmodulesettings")]
        public async Task<ActionResult<bool>> SetModuleSettings(BodySetModuleSettings parameters)
        {
            if (parameters.settings.Count==0)
                return NotFound(false);
            var result = await _misterManager.SetModuleSettings(parameters.settings);

            if (!result)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("checkmodulesettings")]
        public async Task<ActionResult<MisterModuleHealthCheck>> CheckModuleSettings(BodySetModuleSettings parameters)
        {
            if (parameters.settings.Count == 0)
                return NotFound(false);
            var result = await _misterManager.CheckModuleSettings(parameters.settings);

            if (result==null)
                return NotFound();
            return Ok(result);
        }

       
        [HttpPost("automaticmatchrom")]
        public async Task<ActionResult<bool>> AutomaticMatchRomJob(BodyAutomaticMatchRom parameters)
        {
            var result = _misterManager.AutomaticLinkRomToVideoGame(parameters.systemid, parameters.filterresultcode);
            return Ok(result);
        }

        [HttpGet("currentjob")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<JobMister?>> GetCurrentJob()
        {
            var t = _misterManager.CurrentJob();
            if (t==null)
            {
                return NotFound();
            }
            return Ok(t);
        }

        [HttpGet("scripts")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<RemoteServiceScriptResult>> GetScripts()
        {
            var t = await _misterManager.GetScripts();
            if (t == null)
            {
                return NotFound();
            }
            return Ok(t);
        }

        [HttpPost("executescript")]
        public async Task<ActionResult<bool>> ExcecuteScript(BodyExcecuteScript parameters)
        {
            var result = await _misterManager.ExecuteScript(parameters.name, parameters.force);
            return Ok(result);
        }

    }
}
