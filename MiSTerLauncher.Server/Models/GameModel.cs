using libMisterLauncher.Entity;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace apiGameDb.Models
{
    public class GameModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? _id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; } = null!;
        public string desc { get; set; } = null!;
        public string fullpath { get; set; } = null!;
        public string systemname_sc { get; set; } = null!;
        public string editorname_sc { get; set; } = null!;
        public string nbplayers { get; set; } = null!;
        public string developname_sc { get; set; } = null!;
        public string media_fanart { get; set; } = null!;
        public string media_video { get; set; } = null!;
        public string media_manuel { get; set; } = null!;
        public string media_screenshot { get; set; } = null!;
        public string media_title { get; set; } = null!;

    }


    public class LaunchPost
    {
        public string Path { get; set; } = "";
    }

    public class LaunchVideoGamePost
    {
        public string videoGameId { get; set; } = "";
        public string romId { get; set; } = "";

    }

    public class PlaylistPost
    {
        public string GameId { get; set; } = "";
        public string Playlist { get; set; } = "";
        public bool Add { get; set; } = true;


    }

    public class RomActionPost
    {
        public string videogameid { get; set; } = "";
        public string romid { get; set; } = "";
    }

    public class BodyKeyboardCmdPost
    {
        public List<string> cmds { get; set; } = new List<string>();
        public bool raw { get; set; } = true;
        public int delay { get; set; } = 100;
    }

    public class BodyIdPost
    {
        public string Id { get; set; } = "";
    }

    public class BodySavestateCmd
    {
        public string videogameid { get; set; } = "";
        public string romid { get; set; } = "";

        public int slot { get; set; } = 1;



    }

    public class BodyGetSavestate
    {
        public string videogameid { get; set; } = "";
        public string romid { get; set; } = "";
    }

    public class BodyGetModuleSettings
    {
        public string modulename { get; set; } = "";
    }

    public class BodySetModuleSettings
    {
        public List<ModuleSetting> settings { get; set; } = new List<ModuleSetting>();
    }

    public class BodyScanRomConsole
    {
        public string systemid { get; set; } = "";
    }

    public class BodyAutomaticMatchRom
    {
        public string systemid { get; set; } = "";
        public List<int> filterresultcode { get; set; } = new List<int>();

    }

    public class BodyDeleteRom : BodyIdPost
    {
        public bool deletefile { get; set; } = false;
    }

    public class BodyUnmatchRom
    {
        public string category { get; set; } = "";
        public string systemid { get; set; } = "";
    }

    public class VideoGameSearchFromScrapperPost
    {
        public string searchName { get; set; }
        public string systemid { get; set; }
    }

    public class BodyLinkRomToScrapperVideogame
    {
        public string romid { get; set; } = "";
        public int scrapperVideogame { get; set; } = 0;
        public List<string> childroms { get; set; } = new List<string>();
    }

    public class BodyCountFilter
    {
        public string filter { get; set; } = "";
    }

    public class LoginModel
    {
        public string password { get; set; } = "";
    }

    public class GuestAccessParams
    {
        public string signature { get; set; } = "";
    }

    public class GuestAccessCodeParams
    {
        public string code { get; set; } = "";
    }
    public class GuestAccessConsumeParams
    {
        public string code { get; set; } = "";
        public string key { get; set; } = "";
    }

    public class GuestAccessActionParams
    {
        public bool approuved { get; set; } = false;
        public string code { get; set; } = "";

    }

    public class BodyExcecuteScript
    {
        public bool force { get; set; } = false;
        public string name { get; set; } = "";

    }



}
