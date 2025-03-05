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
    public class VideoGameController : ControllerBase
    {
        private MisterManager _misterManager;
        private MisterBackgroundTask _hostedservice;

        public VideoGameController (ILogger<HealthcheckController> logger, MisterBackgroundTask hostedService) : base()
        {
            _hostedservice = hostedService;
            hostedService.Wakeup();
            _misterManager = hostedService.manager;
        }

        [HttpPost("search")]
        [Authorize]
        public async Task<ActionResult<VideoGameSearchResult>> Search(VideoGameSearchRequest search)
        {
            var result = await _misterManager.GetVideoGameResultAsync(search);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("id")]
        public async Task<ActionResult<VideoGameDb>> Details(BodyIdPost idParameters)
        {
            //romid = System.Net.WebUtility.UrlDecode(romid);
            var result = await _misterManager.GetVideoGame(idParameters.Id);
            if (result == null )
                return NotFound();
            return Ok(result);
        }

        [HttpPost("launch")]
        public async Task<ActionResult<VideoGameDb>> Launch(LaunchVideoGamePost parameters)
        {
            var result = await _misterManager.LaunchVideoGame(parameters.videoGameId, parameters.romId);
            // Force refresh to update cache whith current game
            _hostedservice.ForceRefresh(5000);
            return result!=null ? Ok(result) : NotFound();
        }

        [HttpPost("playing")]
        public async Task<ActionResult<PlayingGame>> Playing()
        {
            var result = await _misterManager.CurrentVideogame();
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("playlist")]
        public async Task<ActionResult<VideoGameDb>> SetPlaylist(PlaylistPost playlistParameters)
        {
            var result = await _misterManager.SetVideoGamePlaylist(playlistParameters.GameId, playlistParameters.Playlist, playlistParameters.Add);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("unlinkrom")]
        public async Task<ActionResult<VideoGameDb>> UnlinkRom(RomActionPost parameters)
        {
            var result = await _misterManager.RomActionRomForVideogame(parameters.videogameid, parameters.romid, libMisterLauncher.Service.RomAction.UNLINK);
            return result != null ? Ok(result) : NotFound();
        }
        [HttpPost("linkrom")]
        public async Task<ActionResult<VideoGameDb>> LinkRom(RomActionPost parameters)
        {
            var result = await _misterManager.RomActionRomForVideogame(parameters.videogameid, parameters.romid, libMisterLauncher.Service.RomAction.LINK);
            return result != null ? Ok(result) : NotFound();
        }
        [HttpPost("setprimaryrom")]
        public async Task<ActionResult<VideoGameDb>> SetPrimaryRom(RomActionPost parameters)
        {
            var result = await _misterManager.RomActionRomForVideogame(parameters.videogameid, parameters.romid, libMisterLauncher.Service.RomAction.SETPRIMARY);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("delete")]
        public async Task<ActionResult<bool>> Delete(BodyIdPost parameters)
        {
            var result = await _misterManager.DeleteVideoGame(parameters.Id);
            return result ? Ok(result) : NotFound();
        }

        [HttpPost("updatesettings")]
        public async Task<ActionResult<SystemDb>> UpdateSettings(VideoGameDb videogame)
        {
            var result = await _misterManager.UpdateSettingsVideogame(videogame);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("searchvideogamefromscrapper")]
        public async Task<ActionResult<List<VideoGameDb>>> SearchVideoGameFromScrapper(VideoGameSearchFromScrapperPost parameters)
        {
            var result = await _misterManager.SearchScrapperVideoGameFromSearchName(parameters.searchName, parameters.systemid);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("searchvideogamefromromid")]
        public async Task<ActionResult<VideoGameDb>> SearchVideoGameFromRomId(BodyIdPost parameters)
        {
            var result = await _misterManager.SearhVideogameByRomid(parameters.Id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("getsearchvideogamefilter")]
        public async Task<ActionResult<SearchVideoGameFilter>> GetSearchVideoGameFilter()
        {
            var result = await _misterManager.GetVideoGameSearchFilter();
            return Ok(result);
        }

    }
}
