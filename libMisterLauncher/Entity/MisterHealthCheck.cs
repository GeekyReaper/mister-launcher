using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MisterStateEnum {
        //[EnumMember(Value = "OK")]
        OK,
        //[EnumMember(Value = "WARNING")]
        WARNING,
        //[EnumMember(Value = "ERROR")]
        ERROR,
        //[EnumMember(Value = "NOINI")]
        NOINI }
    public class MisterHealthCheck
    {
        public List<MisterModuleHealthCheck> ModuleHealthchecks { get; } = new List<MisterModuleHealthCheck>();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MisterStateEnum MisterState
        {
            get
            {
                var mongo = ModuleHealthchecks.Where(m => m.Name == "MongoDb").FirstOrDefault();
                if (mongo == null || mongo.MisterState == MisterStateEnum.ERROR)
                {
                    return MisterStateEnum.ERROR;
                }
                var remote = ModuleHealthchecks.Where(m => m.Name == "MisterRemote").FirstOrDefault();
                if (remote == null || remote.MisterState == MisterStateEnum.ERROR)
                {
                    return MisterStateEnum.WARNING;
                }

                return MisterStateEnum.OK;

            }
        }


        public List<string> Messages { get
            {
                return  (from m in ModuleHealthchecks where m.MisterState != MisterStateEnum.OK select string.Format("[{0}] {1}", m.Name, m.Message)).ToList();                
            } }

        public MisterHealthCheck()
        {

        }
        public void AddModuleHealth (MisterModuleHealthCheck module)
        {
            ModuleHealthchecks.RemoveAll ( m => m.Name == module.Name );
            ModuleHealthchecks.Add(module);
            ModuleHealthchecks.OrderBy(m => m.Name);


        }

    }

    public class MisterModuleHealthCheck
    {
        public string Name { get; set; }
        public string Message { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MisterStateEnum MisterState { get; set; } = MisterStateEnum.NOINI;

        public MisterModuleHealthCheck (string name)
        {
            Name = name;
        }
    }

    public class MisterStats
    {
        public long videogamesCount { get; set; } = 0;
        public long systemsCount { get; set; } = 0;

        public long systemsCountWithVideogames { get; set; } = 0;
        public long romsCount { get; set; } = 0;
        public long romsCountMatch { get; set; } = 0;

        public long mediaCount { get; set; } = 0;
        public long mediaDownloadCount { get; set; } = 0;
        public long mediaDownloadSize { get; set; } = 0;




    }

    public class MisterPlayingGame
    {
        public bool Haschanged { get; set; } = false;       
        public PlayingGame CurrentVideoGame { get; set; } = new PlayingGame();

        public string LastGame { get; set; } = "";

        public void SetPlayingGame(PlayingGame game)
        {
            Haschanged = LastGame != game.getId();
            CurrentVideoGame = game;
        }
    }
}
