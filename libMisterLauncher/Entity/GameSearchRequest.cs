using libMisterLauncher.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class SortDb
    {
        public string Field { get; set; }
        public bool IsAscending { get; set; } = true;

        public SortDb(string field, bool isAscending = true)
        {
            Field = field;
            IsAscending = isAscending;
        }
    }

    public class GameSearchRequest
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Regex { get; set; } = string.Empty;
        public List<string> Systems { get; set; } = new List<string>();

        public string NbPlayers { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public int YearMax { get; set; } = 0;
        public int YearMin { get; set; } = 0;
        public bool AllowUnknowYear { get; set; } = false;
        public bool AllowUnRated { get; set; } = false;
        public int MinRating { get; set; } = 0;
        public string SystemCategory { get; set; } = string.Empty;

        public bool Matchscreenscraper { get; set; } = true;
        public int MaxRating { get; set; } = 0;
        public string Editor { get; set; } = string.Empty;
        public string Developname { get; set; } = string.Empty;

        public string Playlist { get; set; } = string.Empty;

        public List<string> GameType { get; set; } = new List<string>();
        public List<string> GameTypeExcluded { get; set; } = new List<string>();

        public int CollectionId { get; set; } = 0;

        public string Core { get; set;} = string.Empty;

        public List<string> GamesExcluded { get; set; } = new List<string>();
        public List<string>SystemsExcluded { get; set; } = new List<string>();


        public int? Limit { get; set; } = 50;

        public List<SortDb> SortFields { get; set; } = new List<SortDb>();


        public GameSearchRequest Clone()
        {
            var copy = new GameSearchRequest();
            copy.Name = Name;
            copy.Regex = Regex;
            copy.NbPlayers = NbPlayers;
            copy.Year = Year;
            copy.YearMax = YearMax;
            copy.YearMin = YearMin;
            copy.AllowUnknowYear = AllowUnknowYear;
            copy.AllowUnRated = AllowUnRated;
            copy.MinRating = MinRating;
            copy.SystemCategory = SystemCategory;
            copy.Matchscreenscraper = Matchscreenscraper;
            copy.MaxRating = MaxRating;
            copy.Editor = Editor;
            copy.Developname = Developname;

            foreach (var s in Systems)
            {
                copy.Systems.Add(s);
            }
            foreach (var s in GameType)
            {
                copy.GameType.Add(s);
            }
            foreach (var s in SortFields)
            {
                copy.SortFields.Add(new SortDb(s.Field, s.IsAscending));
            }


            return copy;
        }
        
    }




    public class GameSystemSearch
    {
        public string Name { get; set; } = string.Empty;

        public string Regex { get; set; } = string.Empty;
        public int Year { get; set; } = 0;
        public int YearMax { get; set; } = 0;
        public int YearMin { get; set; } = 0;
        public bool AllowUnknowYear { get; set; } = true;
        public string Category { get; set; } = string.Empty;
        public string Family { get; set; } = string.Empty;
        public int Generation { get; set; } = 0;
        public string Company { get; set; } = string.Empty;

        public bool AllowNoVideoGame { get; set; } = false;


        public int? Limit { get; set; } = null;

        public List<SortDb> SortFields { get; set; } = new List<SortDb>();


    }

    public class GameSearchResult
    {
        public List<GameResult> games { get; set; } = new List<GameResult>();
        public int count { get; set; } = 0;
        public FilterOption? filterOption { get; set; }
        public List<RecommendedGameSearchRequest> recommendedGameSearchRequests { get; set;} = new List<RecommendedGameSearchRequest>();
    }  

    

    public class SystemSearchResult
    {
        public int count { get; set; } = 0;
        public List<SystemDb> systems { get; set; } = new List<SystemDb>();
    }

    public class GameResultFilterOption
    {
        public List<string> gameTypes { get; } = new List<string>();
        public List<string> nbPlayers { get; } = new List<string>();
         public List<SystemDb> systemDbs { get; } = new List<SystemDb>();

        public int MinYear { get; set; } = 0;
        public int MaxYear { get; set; } = 0;
        public List<string> SystemCategory { get; set; } = new List<string>();

        public void AddGameType(string gametype)
        {
            if (!gameTypes.Contains(gametype))
            {
                gameTypes.Add(gametype);
            }
        }

        public void AddNbPlayers(string nbplayers)
        {
            if (!nbPlayers.Contains(nbplayers))
            {
                nbPlayers.Add(nbplayers);
            }
        }

        public void AddSystemCategory(string category)
        {
            if (!SystemCategory.Contains(category))
            {
                SystemCategory.Add(category);
            }
        }

        public bool GameSystemsExist(string systemid)
        {
            return systemDbs.Where(s => s._id == systemid).Any();
        }

        public void AddGameSystem(SystemDb? gameSystem)
        {
            if (gameSystem != null)
            {
                systemDbs.Add(gameSystem);
            }
        }
    }

    public class RecommendedGameSearchRequest
    {
      public string Name { get; set; }
      public string Category { get; set; }
      public GameSearchRequest gameSearchRequest { get; set; }

      public RecommendedGameSearchRequest (string name, string category, GameSearchRequest search)
        {
            this.Name = name;
            this.Category = category;
            this.gameSearchRequest = search.Clone();
        }

    }

    public class GameAction
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";

        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
        public string Path { get; set; } = "";
        public string Method { get; set; } = "POST";

        public string getKey ()
        {
            return string.Format("{0}/{1}", Category, Name);
        }

        public void SetCommandApi (Dictionary<string, CommandApi> cmds)
        {
            var key = getKey();
            if (cmds.ContainsKey(getKey()))
            {
                Path = cmds[key].Path;
                Method = cmds[key].Method;
            }
        }

    }

    public class GameResult
    {
        public Game? gameDb { get; set; }
        public List<GameAction> gameActions { get; set; } = new List<GameAction>();
    }

    public class GameDetailResult
    {
        public GameFull? gameDb { get; set; }
        public List<GameAction> gameActions { get; set; } = new List<GameAction>();
    }


}
