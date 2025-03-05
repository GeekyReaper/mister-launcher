using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using System.Text;
using System.Security.Authentication.ExtendedProtection;
using MongoDB.Bson.Serialization.Serializers;
using System.Xml.Linq;
using Amazon.Util.Internal;
using System.Net.NetworkInformation;
using libMisterLauncher.Entity;
using MongoDB.Bson.Serialization.IdGenerators;
using static MongoDB.Driver.WriteConcern;
using libMisterLauncher.Manager;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.Arm;
using Amazon.Auth.AccessControlPolicy;
using System.Linq;
using System.Globalization;
using System.Text.RegularExpressions;
using MongoDB.Driver.Linq;
using Amazon.Runtime.SharedInterfaces;

[assembly: InternalsVisibleTo("MisterLauncher.Test")]
namespace libMisterLauncher.Service
{
    public class GameDbSettings : IMisterSettings
    {
        const string _moduleName = "MongoDb";
        public string ModuleName { get { return _moduleName; } }
        public string connectionstring { get; set; } = "";
        public string dbname { get; set; } = "retrogaming";
        public string collectionNameVideoGames { get; set; } = "videogames";
        public string collectionNameMedia { get; set; } = "multimedia";
        public string collectionNameRoms { get; set; } = "roms";
        public string collectionNameSystems { get; set; } = "systems";
        public string collectionNameCoreSaveState { get; set; } = "coresavestates";

        public string collectionNameModuleSettings { get; set; } = "modulesettings";
        public bool isValid()
        {
            return !string.IsNullOrEmpty(connectionstring);
        }

        List<ModuleSetting> IMisterSettings.GetModuleSettings()
        {
            throw new NotImplementedException();
        }

        void IMisterSettings.LoadModuleSettings(List<ModuleSetting> moduleSettings)
        {
            throw new NotImplementedException();
        }
    }

    public enum EnumCollection { ROM, VIDEOGAME, SYSTEM, MEDIA, CORESAVESTATE, MODULESETTINGS}


    internal class GamedbService : BaseModule<GameDbSettings>
    {
        Dictionary<EnumCollection, string> CollectionName = new Dictionary<EnumCollection, string>();

        private IMongoClient _client;
        private IMongoDatabase _database;      
        public GamedbService(GameDbSettings? settings) : base (settings)
        {
            _client = new MongoClient(_settings.connectionstring);
            _database = _client.GetDatabase(_settings.dbname);
            SetCollectionName();


        }

        internal void SetCollectionName()
        {
            var c = new Dictionary<EnumCollection, string>();
            c.Add(EnumCollection.MEDIA, _settings.collectionNameMedia);
            c.Add(EnumCollection.VIDEOGAME, _settings.collectionNameVideoGames);
            c.Add(EnumCollection.ROM, _settings.collectionNameRoms);
            c.Add(EnumCollection.SYSTEM, _settings.collectionNameSystems);
            c.Add(EnumCollection.CORESAVESTATE, _settings.collectionNameCoreSaveState);
            c.Add(EnumCollection.MODULESETTINGS, _settings.collectionNameModuleSettings);
            CollectionName = c;
        }

        #region Convert JSon to Object
        public int importLargeJson(string filepath, string collectionname)
        {
            //var sys = new List<Newtonsoft.Json.Linq.JObject>();
            var count = 0;
            var collection = _database.GetCollection<BsonDocument>(collectionname);
            //string value = System.IO.File.ReadAllText(filepath);
            using (Stream stream = File.OpenRead(filepath))
            using (StreamReader streamReader = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(streamReader))
            {
                reader.SupportMultipleContent = true;

                var serializer = new Newtonsoft.Json.JsonSerializer();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.StartObject)
                    {

                        //sys.Add(serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader));
                        var item = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader);
                        var jsontext = item.ToString();
                        var document = BsonSerializer.Deserialize<BsonDocument>(jsontext);
                        var filter = Builders<BsonDocument>.Filter.Eq(d => d["_id"], document.GetElement("_id").Value.AsString);
                        var r = collection.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
                        if (r.MatchedCount == 1)
                        {
                            Console.WriteLine("Replace : " + document.GetElement("_id").Value.AsString);
                        }
                        else
                        {
                            Console.WriteLine("Insert : " + r.UpsertedId.AsString);
                        }


                        count++;
                    }
                }
            }

            //foreach (var item in sys)
            //for (int i =0; i<20; i++)
            //{
            //    var jsontext = sys[i].ToString();
            //    var document = BsonSerializer.Deserialize<BsonDocument>(jsontext);
            //    var filter = Builders<BsonDocument>.Filter.Eq(d => d["_id"], document.GetElement("_id").Value.AsString);
            //    collectionVideogame.ReplaceOne(filter, document, new ReplaceOptions { IsUpsert = true });
            //    //collectionVideogame.InsertOne(document);
            //    //collectionVideogame.InsertOne(document);

            //    //var update = Builders<BsonDocument>.Update.Set(d => d, document);
            //    //UpdateOptions opts = new UpdateOptions()
            //    //{
            //    //    IsUpsert = true
            //    //};
            //    //var r = collectionVideogame.UpdateOne(filter, update, opts);
            //    //if (r.ModifiedCount == 0)
            //    //{
            //    //    Console.WriteLine("Insert : " + r.UpsertedId.AsString);
            //    //}
            //    //else
            //    //{
            //    //    Console.WriteLine("Alreadyexist : " + document.GetElement("_id").Value.AsString);
            //    //}
            //}


            return count;
        }

        public async Task<int> RefreshEnumGameType(string name, string enumcollectionname, string gamecollection)
        {
            var gametype = new Dictionary<string, int>();
            var enumcollection = _database.GetCollection<GameEnum>(enumcollectionname);
            var collection = _database.GetCollection<Game>(gamecollection);
            var games = await collection.Find(Builders<Game>.Filter.Empty).ToListAsync();
            games.ForEach(g =>
            {

                g.gametype?.ForEach(gt =>
                {
                    if (gametype.ContainsKey(gt))
                    {
                        gametype[gt]++;
                    }
                    else
                    {
                        gametype.Add(gt, 0);
                        var r = enumcollection.InsertOneAsync(new GameEnum { name = name, value = gt });

                    }
                }
                );
            });
            //var filter = Builders<GameEnum>.Filter.Eq(e => e.name, name);
            //var r = enumcollection.ReplaceOne(filter, new GameEnum {  _id = Guid.NewGuid(), values = gametype.Select(i  => i.Key).OrderBy(f => f).ToList() } , new ReplaceOptions { IsUpsert = true });
            return gametype.Count;
            //enumcollection.INse
        }

        public BsonDocument GetGenericDocumentById(string id, string collectionname)
        {
            var collection = _database.GetCollection<BsonDocument>(collectionname);
            var filter = Builders<BsonDocument>.Filter.Eq(d => d["_id"], id);

            return collection.Find<BsonDocument>(filter).FirstOrDefault();
        }

        private ClassGenerator GetDeserializeClass(BsonDocument document, string classname, ref Dictionary<string, ClassGenerator> ChildClass)
        {
            var currentClass = new ClassGenerator(classname);
            int i = 1;
            foreach (var e in document.Elements)
            {
                var proptype = document.GetElement(e.Name).Value.GetType().Name;
                if (proptype != "BsonArray" && proptype != "BsonDocument")
                {
                    var prop = new PropertyGenerator(e.Name, proptype);
                    currentClass.AddProperty(prop);
                }
                else if (proptype == "BsonArray")
                {

                    if (document.GetElement(e.Name).Value.AsBsonArray[0].IsBsonDocument)
                    {
                        //var subclass = GetDeserializeClass(document.GetElement(e.Name).Value.AsBsonArray[0].AsBsonDocument, "sub" + classname + "_" + i, ref ChildClass);
                        ClassGenerator subclass = null;
                        foreach (var item in document.GetElement(e.Name).Value.AsBsonArray)
                        {
                            var subitemclass = GetDeserializeClass(item.AsBsonDocument, "sub" + classname + "_" + i, ref ChildClass);
                            if (subclass == null)
                            {
                                subclass = subitemclass;
                            }
                            else
                            {
                                foreach (var key in subitemclass.Properties.Keys)
                                {
                                    subclass.AddProperty(subitemclass.Properties[key]);
                                }
                            }
                        }

                        if (!ChildClass.ContainsKey(subclass.GetPropertiesKey()))
                        {
                            ChildClass.Add(subclass.GetPropertiesKey(), subclass);
                        }
                        else
                        {
                            subclass = ChildClass[subclass.GetPropertiesKey()];
                        }
                        var prop = new PropertyGenerator(e.Name, "List<" + subclass.Name + ">");
                        currentClass.AddProperty(prop);
                    }
                    if (document.GetElement(e.Name).Value.AsBsonArray[0].IsString)
                    {
                        var prop = new PropertyGenerator(e.Name, "List<string>");
                        currentClass.AddProperty(prop);
                    }


                }
                else if (proptype == "BsonDocument")
                {
                    var subclass = GetDeserializeClass(document.GetElement(e.Name).Value.AsBsonDocument, "sub" + classname + "_" + i, ref ChildClass);
                    if (!ChildClass.ContainsKey(subclass.GetPropertiesKey()))
                    {
                        ChildClass.Add(subclass.GetPropertiesKey(), subclass);
                    }
                    else
                    {
                        subclass = ChildClass[subclass.GetPropertiesKey()];
                    }
                    var prop = new PropertyGenerator(e.Name, subclass.Name);
                    currentClass.AddProperty(prop);
                }
                i++;
            }
            return currentClass;
        }


        public List<ClassGenerator> GetDeserializeClass(BsonDocument document, string classname)
        {
            var childClass = new Dictionary<string, ClassGenerator>();
            var parentClass = GetDeserializeClass(document, classname, ref childClass);
            var result = new List<ClassGenerator>
            {
                parentClass
            };
            foreach (var key in childClass.Keys)
            {
                result.Add(childClass[key]);
            }
            return result;
        }
        #endregion

        public override bool CheckConnection()
        {
            var command = (Command<BsonDocument>)"{ping:1}";

            var result = _database.RunCommandAsync(command).Wait(5000);
            return result;
        }

        

        #region Game methods

        //public async Task<BsonDocument?> GetGameRaw(string id)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return (BsonDocument?)null;
        //    }
        //    IMongoCollection<BsonDocument> collectionVideogame = _database.GetCollection<BsonDocument>("games");
        //    var games = await collectionVideogame.Find(g => g["_id"] == id).FirstOrDefaultAsync<BsonDocument>();
        //    return games;
        //}

        //public async Task<GameFull?> GetGameDetail(string id)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return (GameFull?)null;
        //    }
        //    IMongoCollection<GameFull> collectionVideogame = _database.GetCollection<GameFull>("games");
        //    var games = await collectionVideogame.Find(g => g._id == id).FirstOrDefaultAsync<GameFull>();
        //    return games;
        //}


        //private async Task<bool> SetPlaylistGame(Game game)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return false;
        //    }
        //    IMongoCollection<Game> collectionVideogame = _database.GetCollection<Game>("games");
        //    var update = Builders<Game>.Update.Set(g => g.playlist, game.playlist);

        //    var r = await collectionVideogame.UpdateOneAsync(g => g._id == game._id, update, new UpdateOptions { IsUpsert = false });

        //    return r.MatchedCount == 1;
        //}
        //public async Task<Game?> GetGame(string id)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return (Game?)null;
        //    }
        //    IMongoCollection<Game> collectionVideogame = _database.GetCollection<Game>("games");
        //    var games = await collectionVideogame.Find(g => g._id == id).FirstOrDefaultAsync<Game>();
        //    return games;
        //}
        //private FilterDefinition<Game> GetFilter(GameSearchRequest search)
        //{


        //    if (search.Id != string.Empty)
        //    {
        //        return Builders<Game>.Filter.Eq(g => g._id, search.Id);               
        //    }

        //    var filter = Builders<Game>.Filter.Eq(g => g.matchscreenscraper, search.Matchscreenscraper);


        //    if (search.Name != string.Empty && search.Regex == string.Empty)
        //    {
        //        search.Regex = ".*" + search.Name.Replace(" ", ".*") + ".*";
        //    }

        //    if (search.Regex != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Regex(g => g.name, new BsonRegularExpression(search.Regex, "i"));
        //    }

        //    if (search.SystemId != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.systemid, search.SystemId);
        //    }
            
        //    if (search.NbPlayers != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.nbplayers, search.NbPlayers);
        //    }
        //    if (search.SystemCategory != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.systemcategory, search.SystemCategory);
        //    }

        //    if (search.Playlist != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => g.playlist!= null && g.playlist.Contains(search.Playlist));
        //    }

        //    if (search.ScGameType.Count > 0)
        //    {
        //        FilterDefinition<Game>? f = null;
        //        foreach (var gametype in search.ScGameType)
        //        {
        //            if (f==null)
        //            {
        //                f = Builders<Game>.Filter.Where(g => g.gametype != null && g.gametype.Contains(gametype));
        //            }
        //            else
        //            {
        //                f = f | Builders<Game>.Filter.Where(g => g.gametype != null && g.gametype.Contains(gametype));
        //            }
        //        }
        //        filter = filter & f;
        //    }

        //    if (search.GameTypeExcluded.Count > 0)
        //    {
        //        FilterDefinition<Game>? f = null;
        //        foreach (var gametype in search.GameTypeExcluded)
        //        {
        //            if (f == null)
        //            {
        //                f = Builders<Game>.Filter.Where(g => g.gametype != null && !g.gametype.Contains(gametype));
        //            }
        //            else
        //            {
        //                f = f & Builders<Game>.Filter.Where(g => g.gametype != null && !g.gametype.Contains(gametype));
        //            }
        //        }
        //        filter = filter & f;
        //    }

        //    if (search.GamesExcluded.Count > 0)
        //    {
        //        FilterDefinition<Game>? f = null;
        //        foreach (var gameid in search.GamesExcluded)
        //        {
        //            if (f == null)
        //            {
        //                f = Builders<Game>.Filter.Where(g => g._id != gameid);
        //            }
        //            else
        //            {
        //                f = f & Builders<Game>.Filter.Where(g => g._id != gameid);
        //            }
        //        }
        //        filter = filter & f;
        //    }

        //    if (search.SystemsExcluded.Count > 0)
        //    {
        //        FilterDefinition<Game>? f = null;
        //        foreach (var systemid in search.SystemsExcluded)
        //        {
        //            if (f == null)
        //            {
        //                f = Builders<Game>.Filter.Where(g => g.systemid != systemid);
        //            }
        //            else
        //            {
        //                f = f & Builders<Game>.Filter.Where(g => g.systemid != systemid);
        //            }
        //        }
        //        filter = filter & f;
        //    }

        //    if (search.CollectionId != 0)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => g.collectionId == search.CollectionId);
        //    }

        //    if (!string.IsNullOrEmpty(search.Core))
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => g.core.ToLower() == search.Core.ToLower());
        //    }


        //    if (!search.AllowUnknowYear)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => g.year > 0);
        //    }

        //    if (!search.AllowUnRated)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => g.rating > -1);
        //    }

        //    if (search.Year != 0)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.year, search.Year);
        //    }

        //    if (search.Editor != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.editorname, search.Editor);
        //    }
        //    if (search.Developname != string.Empty)
        //    {
        //        filter = filter & Builders<Game>.Filter.Eq(g => g.developname, search.Developname);
        //    }


        //    if (search.MaxRating > 0 || search.MinRating > 0)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => (search.MaxRating == 0 || g.rating <= search.MaxRating) && (search.MinRating == 0 || g.rating >= search.MinRating));
        //    }

        //    if (search.YearMax > 0 || search.YearMin > 0)
        //    {
        //        filter = filter & Builders<Game>.Filter.Where(g => (search.YearMax == 0 || g.year <= search.YearMax) && (search.YearMin == 0 || g.year >= search.YearMin));
        //    }

            


        //    return filter;
        //}      

        //public List<Game> GetMatchGame(GameSearchRequest search)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return new List<Game>();
        //    }
        //    IMongoCollection<Game> collectionVideogame = _database.GetCollection<Game>("games");
        //    if (search.Id != string.Empty)
        //    {
        //        search.pagesize = 1;
        //    }
        //    return collectionVideogame.Find(GetFilter(search)).Sort(GetSort<Game>(search.SortFields)).pagesize(search.pagesize).ToList();
        //}

        //public FilterOption GetFilter(List<Game> games, GameSearchRequest search)
        //{

        //    var filter = new FilterOption
        //    {
        //        MaxYear = int.MinValue,
        //        MinYear = int.MaxValue
        //    };
        //    foreach (var game in games)
        //    {
        //        if (game.year < filter.MinYear)
        //            filter.MinYear = game.year;
        //        if (game.year > filter.MaxYear)
        //            filter.MaxYear = game.year;
        //        game.gametype.ForEach(g => filter.AddGameType(g));
        //        filter.AddNbPlayers(game.nbplayers);
        //        filter.AddSystemCategory(game.systemcategory);
        //        if (!filter.GameSystemsExist(game.systemid))
        //        {
        //            filter.AddGameSystem(GetSystemDb(game.systemid).Result);
        //        }
        //    }

        //    return filter;
        //}

        //public List<RecommendedGameSearchRequest> GetRecommandFilter(List<Game> games, GameSearchRequest search)
        //{
        //    var result = new List<RecommendedGameSearchRequest>();
        //    var listfilter = new List<RecommendedGameSearchRequest>();
        //    // Add System Filter
        //    foreach (var system in (from g in games select new { g.systemid, g.systemname }).Distinct())
        //    {
        //        var rfilter = new RecommendedGameSearchRequest(system.systemname, "System", search);
        //        rfilter.gameSearchRequest.SystemId = system.systemid;
        //        listfilter.Add(rfilter);
        //    }
        //    if (listfilter.Count > 1)
        //    {
        //        result.AddRange(listfilter);
        //    }

        //    // Add ScGameType Filter
        //    listfilter.Clear();
        //    var listgametype = new List<string>();
        //    foreach (var gamestype in (from g in games select g.gametype))
        //    {
        //        listgametype.AddRange(gamestype);
        //    }
        //    foreach (var gt in listgametype.Distinct())
        //    {
        //        var rfilter = new RecommendedGameSearchRequest(gt, "ScGameType", search);
        //        rfilter.gameSearchRequest.ScGameType.Add(gt);
        //        listfilter.Add(rfilter);
        //    }
        //    if (listfilter.Count > 1)
        //    {
        //        result.AddRange(listfilter);
        //    }

        //    return result;

        //}

        //public async Task<bool> SetPlaylist(string gamedId, string playlist, bool add = true)
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return false;
        //    }
        //    var game = await GetGame(gamedId);
        //    if (game == null)
        //        return false;
        //    if (add)
        //    {
        //        if (game.playlist == null)
        //            game.playlist = new List<string>() { playlist };
        //        else if (!game.playlist.Contains(playlist))
        //            game.playlist.Add(playlist);
        //    }
        //    else
        //    {
        //        if ((game.playlist != null) && (game.playlist.Contains(playlist)))
        //        {
        //            game.playlist.Remove(playlist);
        //            if (game.playlist.Count == 0)
        //                game.playlist = null;
        //        }
        //    }

        //    return await SetPlaylistGame(game);






        //}

        //public long GetCountGames()
        //{
        //    if (_health.MisterState != MisterStateEnum.OK)
        //    {
        //        return 0;
        //    }
        //    IMongoCollection<Game> collectionVideogame = _database.GetCollection<Game>("games");
        //    var filter = Builders<Game>.Filter.Eq(s => s.matchscreenscraper, true);
        //    return collectionVideogame.CountDocuments(filter);
        //}

        #endregion

        #region SystemDb

        public async Task<SystemDb?> GetSystemDb(string id, string collectionname = "")
        {
            if (string.IsNullOrEmpty(collectionname))
                collectionname = _settings.collectionNameSystems;
            if (string.IsNullOrEmpty(id))
                return null;
            IMongoCollection<SystemDb> collection = _database.GetCollection<SystemDb>(collectionname);
            return await collection.Find(g => g._id == id).FirstOrDefaultAsync<SystemDb>();
        }

        public async Task<SystemDb?> GetSystemDbFromScrapperId(int scrapperId)
        {
           
           var collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);
            return await collection.Find(g => g.screenscraperId == scrapperId).FirstOrDefaultAsync<SystemDb>();
        }
        private FilterDefinition<SystemDb> GetFilter(GameSystemSearch search)
        {

            var filter = Builders<SystemDb>.Filter.Empty;

            if (search.Name != string.Empty && search.Regex == string.Empty)
            {
                search.Regex = ".*" + search.Name.Replace(" ", ".*") + ".*";
            }

            if (search.Regex != string.Empty)
            {
                var filtername = Builders<SystemDb>.Filter.Regex(g => g.name, new BsonRegularExpression(search.Regex, "i"));
                filtername = filtername | Builders<SystemDb>.Filter.Regex(g => g.alternative_name, new BsonRegularExpression(search.Regex, "i"));
                filtername = filtername | Builders<SystemDb>.Filter.Regex(g => g.abreviation, new BsonRegularExpression(search.Regex, "i"));
                filtername = filtername | Builders<SystemDb>.Filter.Regex(g => g.name_eu, new BsonRegularExpression(search.Regex, "i"));
                filtername = filtername | Builders<SystemDb>.Filter.Regex(g => g.name_jp, new BsonRegularExpression(search.Regex, "i"));
                filtername = filtername | Builders<SystemDb>.Filter.Regex(g => g.name_us, new BsonRegularExpression(search.Regex, "i"));
                filter = filter & filtername;

            }

            if (!search.AllowNoVideoGame)
            {
                filter = filter & Builders<SystemDb>.Filter.Gt(g => g.statvideogame, 0);
            }

            if (search.Category != string.Empty)
            {
                filter = filter & Builders<SystemDb>.Filter.Eq(g => g.category, search.Category);
            }

            if (search.Family != string.Empty)
            {
                filter = filter & Builders<SystemDb>.Filter.Where(g => g.family.ToLower() == search.Family.ToLower());
            }

            if (search.Generation != 0)
            {
                filter = filter & Builders<SystemDb>.Filter.Eq(g => g.generation, search.Generation);
            }

            if (search.Year != 0)
            {
                filter = filter & Builders<SystemDb>.Filter.Where(g => g.startyear >= search.Year && g.endyear <= search.Year);
            }



            if (search.YearMax > 0 || search.YearMin > 0)
            {
                filter = filter & Builders<SystemDb>.Filter.Where(g => (search.YearMax == 0 || g.startyear <= search.YearMax) && (search.YearMin == 0 || g.endyear >= search.YearMin));
            }


            return filter;
        }
        private SortDefinition<SystemDb> GetSort(GameSystemSearch search)
        {
            return GetSort<SystemDb>(search.SortFields);
        
        }

        public async Task<List<SystemDb>> GetMatchGameSystem(GameSystemSearch search)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return new List<SystemDb>();
            }
            IMongoCollection<SystemDb> collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);
            return await collection.Find(GetFilter(search)).Sort(GetSort(search)).Limit(search.Limit).ToListAsync();
        }

        public async Task<List<SystemDb>> GetAllSystems()
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return new List<SystemDb>();
            }
            IMongoCollection<SystemDb> collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);
            return await collection.Find(Builders<SystemDb>.Filter.Empty).ToListAsync();
        }      

        public async Task<bool> UpdateSystemDbFull(SystemDbFull systemfull, string collectionname = "")
        {
            if (string.IsNullOrEmpty(collectionname))
                collectionname = _settings.collectionNameSystems;
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<SystemDbFull>(collectionname);

            // Update excluded : GamePAth, Extension
            var update = Builders<SystemDbFull>.Update
                .Set(s => s.name, systemfull.name)
                .Set(s => s.media_BoitierConsole3D, systemfull.media_BoitierConsole3D)
                .Set(s => s.media_controller, systemfull.media_controller)
                .Set(s => s.media_illustration, systemfull.media_illustration)
                .Set(s => s.media_logomonochrome, systemfull.media_logomonochrome)
                .Set(s => s.media_photo, systemfull.media_photo)
                .Set(s => s.media_video, systemfull.media_video)
                .Set(s => s.media_wheel, systemfull.media_wheel)
                .Set(s => s.screenscraperId, systemfull.screenscraperId)
                .Set(s => s.company, systemfull.company)
                .Set(s => s.enddate, systemfull.enddate)
                .Set(s => s.endyear, systemfull.endyear)
                .Set(s => s.startyear, systemfull.startyear)
                .Set(s => s.startdate, systemfull.startdate)
                .Set(s => s.name_eu, systemfull.name_eu)
                .Set(s => s.name_jp, systemfull.name_jp)
                .Set(s => s.name_us, systemfull.name_us)
                .Set(s => s.supporttype, systemfull.supporttype)
                .Set(s => s.systemtype, systemfull.systemtype)
                .Set(s => s.unused, systemfull.unused);

            try
            {
                var updateresult = await collection.UpdateOneAsync(s => s._id == systemfull._id, update, new UpdateOptions { IsUpsert = false });
                if (updateresult.MatchedCount == 0)
                {
                    await collection.InsertOneAsync(systemfull);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;

            //await collectionVideogame.InsertOneAsync(systemfull);
            //return true;
        }

        public async Task<bool> UpdateSystemStatistics (string id, int statVideoGame, int statRomFound, int statRomMatch)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);

            // Update excluded : GamePAth, Extension
            var update = Builders<SystemDb>.Update
                .Set(s => s.statvideogame, statVideoGame)
                .Set(s => s.statromfound, statRomFound)
                .Set(s => s.statrommatch, statRomMatch);
            var updateresult = await collection.UpdateOneAsync(s => s._id == id, update, new UpdateOptions { IsUpsert = false });
            return updateresult.MatchedCount == 1;


        }

        public async Task<SystemDb?> UpdateSettingsSystem (SystemDb uptadesystem)
        {
            var collectionname = _settings.collectionNameSystems;
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return null;
            }
            var collection = _database.GetCollection<SystemDb>(collectionname);

            // Update excluded : GamePAth, Extension
            var update = Builders<SystemDb>.Update
                .Set(s => s.name, uptadesystem.name)
                .Set(s => s.core, uptadesystem.core)
                .Set(s => s.company, uptadesystem.company)
                .Set(s => s.supporttype, uptadesystem.supporttype)
                .Set(s => s.gamepath, uptadesystem.gamepath)
                .Set(s => s.extensions, uptadesystem.extensions)
                .Set(s => s.excluderompaterns, uptadesystem.excluderompaterns)
                .Set(s => s.unofficalpathrompaterns, uptadesystem.unofficalpathrompaterns)
                .Set(s => s.startyear, uptadesystem.startyear)
                .Set(s => s.endyear, uptadesystem.endyear)
                .Set(s => s.allowSaveMemory, uptadesystem.allowSaveMemory)
                .Set(s => s.allowSaveStates, uptadesystem.allowSaveStates);
            try
            {
                var updateresult = await collection.UpdateOneAsync(s => s._id == uptadesystem._id, update, new UpdateOptions { IsUpsert = false });               
            }
            catch (Exception ex)
            {
                return null;
            }



            return await GetSystemDb(uptadesystem._id);
        }

        public async Task<List<ItemCount>>  GetSystemsWithUnmatchRoms()
        {
            var collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            //var agregate = Builders<VideoGameDb>.
            var matchFilter = Builders<Rom>.Filter.Where(r => !r.isMatch);
            // Defines the aggregation pipeline with the $match and $group aggregation stages
            var pipeline = new EmptyPipelineDefinition<Rom>()
                .Match(matchFilter)
                .Group(r => r.systemid,
                    g => new
                    {
                        _id = g.Key,
                        Count = g.Count()
                    }
                );
            var cursor = collection.Aggregate(pipeline).ToList();
            var listofsystemid = new List<ItemCount>();
            foreach (var c in cursor)
            {
                listofsystemid.Add(new ItemCount() { value = string.IsNullOrEmpty(c._id) ? "Arcade" : c._id, count = c.Count, label = string.IsNullOrEmpty(c._id) ? "Arcade" :"" });
                
            }

            var f = Builders<SystemDb>.Filter.Where(g => listofsystemid.Exists( s => s.value == g._id));
            var collectionsystem = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);

            var systems = await collectionsystem.Find(f).ToListAsync();

            foreach (var sys in systems)
            {
                var i = listofsystemid.Where( s => s.value == sys._id).FirstOrDefault();
                if (i!=null)
                {
                    i.label = sys.name;
                }
            }

            

            return listofsystemid.OrderBy(s => s.label).ToList();
        }

        public async Task<List<ItemCount>> GetSystemsWithTotalRoms()
        {
            var collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            //var agregate = Builders<VideoGameDb>.
            var matchFilter = Builders<Rom>.Filter.Empty;
            // Defines the aggregation pipeline with the $match and $group aggregation stages
            var pipeline = new EmptyPipelineDefinition<Rom>()
                .Match(matchFilter)
                .Group(r => r.systemid,
                    g => new
                    {
                        _id = g.Key,
                        Count = g.Count()
                    }
                );
            var cursor = collection.Aggregate(pipeline).ToList();
            var listofsystemid = new List<ItemCount>();
            var arcadeItemCount = new ItemCount() { label = "Arcade", value = "Arcade", count = 0 };
            foreach (var c in cursor)
            {
                if (string.IsNullOrEmpty(c._id) || c._id.ToLower().StartsWith("arcade-"))
                    arcadeItemCount.count += c.Count;
                else
                    listofsystemid.Add(new ItemCount() { value = string.IsNullOrEmpty(c._id) || c._id.StartsWith("Arcade") ? "Arcade" : c._id, count = c.Count, label = string.IsNullOrEmpty(c._id) ? "Arcade" : "" });

            }

            var f = Builders<SystemDb>.Filter.Where(g => g.category == "Console");
            var collectionsystem = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);

            var systems = await collectionsystem.Find(f).ToListAsync();

            foreach (var sys in systems)
            {
                
                var i = listofsystemid.Where(s => s.value == sys._id).FirstOrDefault();
                if (i != null)
                {
                    i.label = sys.name;
                }
                else
                {
                    listofsystemid.Add(new ItemCount() { value = sys._id, count = 0, label = sys.name });
                }
            }
            listofsystemid = listofsystemid.OrderBy(s => s.label).ToList();
            listofsystemid.Insert(0, arcadeItemCount);


            return listofsystemid;
        }

        #endregion

        #region VideoGameDb

        public async Task<Boolean> IncrementVideogamePlayed (string id)
        {
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var update = Builders<VideoGameDb>.Update.Inc(v => v.playedhit, 1).Set(v => v.playedlast, DateTime.Now);
            var updatematch = await collection.UpdateOneAsync(v => v._id == id, update);
            return updatematch.MatchedCount > 0;

        }
        
        public async Task<VideoGameDb?> GetVideoGame(string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return (VideoGameDb?)null;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var games = await collection.Find(g => g._id == id).FirstOrDefaultAsync<VideoGameDb>();
            return games;
        }

        public async Task<bool> DeleteVideoGame(string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var deleteresult = await collection.DeleteOneAsync(v => v._id == id);
            return deleteresult.DeletedCount == 1;
        }

        public async Task<VideoGameDb?> GetVideoGameFomScrapperId(int id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return (VideoGameDb?)null;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var games = await collection.Find(g => g.screenscraperId == id).FirstOrDefaultAsync<VideoGameDb>();
            return games;
        }
        private FilterDefinition<VideoGameDb> GetFilter(VideoGameSearchRequest search)
        {


            if (search.Id != string.Empty)
            {
                return Builders<VideoGameDb>.Filter.Eq(g => g._id, search.Id);
            }

            var filter = Builders<VideoGameDb>.Filter.Empty;


            if (search.Name != string.Empty && search.Regex == string.Empty)
            {
                search.Regex = ".*" + search.Name.ToLower().Replace(" ", ".*") + ".*";
            }

            if (search.Regex != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Regex(g => g.namesearch, new BsonRegularExpression(search.Regex, "i"));
            }

            //if (search.SystemId != string.Empty)
            //{
            //    filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.systemid, search.SystemId);
            //}

            if (search.NbPlayers != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.nbplayers, search.NbPlayers);
            }
            if (search.SystemCategory != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.systemcategory, search.SystemCategory);
            }

            if (search.Playlist != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => g.playlist != null && g.playlist.Contains(search.Playlist));
            }

            if (search.Systems.Count > 0)
            {
                FilterDefinition<VideoGameDb>? f = null;
                foreach (var system in search.Systems)
                {
                    if (f == null)
                    {
                        f = Builders<VideoGameDb>.Filter.Where(g => g.systemid == system);
                    }
                    else
                    {
                        f = f | Builders<VideoGameDb>.Filter.Where(g => g.systemid == system);
                    }
                }
                filter = filter & f;
            }


            if (search.GameType.Count > 0)
            {
                FilterDefinition<VideoGameDb>? f = null;
                foreach (var gametype in search.GameType)
                {
                    if (f == null)
                    {
                        f = Builders<VideoGameDb>.Filter.Where(g => g.gametype != null && g.gametype.Contains(gametype));
                    }
                    else
                    {
                        f = f | Builders<VideoGameDb>.Filter.Where(g => g.gametype != null && g.gametype.Contains(gametype));
                    }
                }
                filter = filter & f;
            }

            if (search.GameTypeExcluded.Count > 0)
            {
                FilterDefinition<VideoGameDb>? f = null;
                foreach (var gametype in search.GameTypeExcluded)
                {
                    if (f == null)
                    {
                        f = Builders<VideoGameDb>.Filter.Where(g => g.gametype != null && !g.gametype.Contains(gametype));
                    }
                    else
                    {
                        f = f & Builders<VideoGameDb>.Filter.Where(g => g.gametype != null && !g.gametype.Contains(gametype));
                    }
                }
                filter = filter & f;
            }

            if (search.GamesExcluded.Count > 0)
            {
                FilterDefinition<VideoGameDb>? f = null;
                foreach (var gameid in search.GamesExcluded)
                {
                    if (f == null)
                    {
                        f = Builders<VideoGameDb>.Filter.Where(g => g._id != gameid);
                    }
                    else
                    {
                        f = f & Builders<VideoGameDb>.Filter.Where(g => g._id != gameid);
                    }
                }
                filter = filter & f;
            }

            if (search.SystemsExcluded.Count > 0)
            {
                FilterDefinition<VideoGameDb>? f = null;
                foreach (var systemid in search.SystemsExcluded)
                {
                    if (f == null)
                    {
                        f = Builders<VideoGameDb>.Filter.Where(g => g.systemid != systemid);
                    }
                    else
                    {
                        f = f & Builders<VideoGameDb>.Filter.Where(g => g.systemid != systemid);
                    }
                }
                filter = filter & f;
            }

            if (!string.IsNullOrEmpty(search.Collection))
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => g.collection.ToLower() == search.Collection.ToLower());
            }

            if (!string.IsNullOrEmpty(search.Core))
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => g.roms!=null && g.roms.Exists(r => r.core.ToLower() == search.Core.ToLower()));
            }


            if (!search.AllowUnknowYear)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => g.year > 1);
            }

            if (!search.AllowUnRated)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => g.rating > -1);
            }

            if (search.Year != 0)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.year, search.Year);
            }

            if (search.Editor != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.editorname, search.Editor);
            }
            if (search.Developname != string.Empty)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.developname, search.Developname);
            }


            if (search.MaxRating > 0 || search.MinRating > 0)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => (search.MaxRating == 0 || g.rating <= search.MaxRating) && (search.MinRating == 0 || g.rating >= search.MinRating));
            }

            if (search.YearMax > 0 || search.YearMin > 0)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => (search.YearMax == 0 || g.year <= search.YearMax) && (search.YearMin == 0 || g.year >= search.YearMin));
            }
            if (search.playedhitMin > -1 || search.playedhitMax > -1)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => (search.playedhitMax == -1 || g.playedhit <= search.playedhitMax) && (search.playedhitMin == -1 || g.playedhit >= search.playedhitMin));
            }
            
            if (search.playedlastMin > DateTime.MinValue || search.playedlastMax> DateTime.MinValue)
            {
                filter = filter & Builders<VideoGameDb>.Filter.Where(g => (search.playedlastMax == DateTime.MinValue || g.playedlast <= search.playedlastMax) && (search.playedlastMin == DateTime.MinValue || g.playedlast >= search.playedlastMin));
            }






            return filter;
        }
        public List<VideoGameDb> GetMatchVideoGame(VideoGameSearchRequest search)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return new List<VideoGameDb>();
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);            

            if (search.Id != string.Empty)
            {
                //search.pagesize = 1;
                return collection.Find(GetFilter(search)).Sort(GetSort<VideoGameDb>(search.SortFields)).Limit(1).ToList();
            }
            if (search.limit >0)
            {
                return collection.Find(GetFilter(search)).Sort(GetSort<VideoGameDb>(search.SortFields)).Limit(search.limit).ToList();
            }
            return collection.Find(GetFilter(search)).Sort(GetSort<VideoGameDb>(search.SortFields)).ToList();

        }
        private async Task<bool> SetPlaylistGame(VideoGameDb videogame)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var update = Builders<VideoGameDb>.Update.Set(g => g.playlist, videogame.playlist);

            var r = await collection.UpdateOneAsync(g => g._id == videogame._id, update, new UpdateOptions { IsUpsert = false });

            return r.MatchedCount == 1;
        }
        public FilterOption GetFilterOption(List<VideoGameDb> videoGames, VideoGameSearchRequest search)
        {

            var filter = new FilterOption();
            
            foreach (var videoGame in videoGames)
            {
                filter.SetYearInterval(videoGame.year);
                videoGame.gametype.ForEach(g => filter.AddValueToCategory( EFilterOptionCategory.GameType ,g));
                filter.AddValueToCategory(EFilterOptionCategory.GameType, videoGame.nbplayers);
                filter.AddValueToCategory(EFilterOptionCategory.Developers, videoGame.developname);
                filter.AddValueToCategory(EFilterOptionCategory.Editors, videoGame.editorname);
                filter.AddValueToCategory(EFilterOptionCategory.Collections, videoGame.collection);
                filter.AddValueToCategory(EFilterOptionCategory.SystemCategory, videoGame.systemcategory);
                
                if (!filter.GameSystemsExist(videoGame.systemid))
                {
                    filter.AddGameSystem(GetSystemDb(videoGame.systemid).Result);
                }
            }

            filter.SetSort();

            return filter;
        }
        public async Task<bool> SetVideoGamePlaylist(string videoGameId, string playlist, bool add = true)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var game = await GetVideoGame(videoGameId);
            if (game == null)
                return false;
            if (add)
            {
                if (game.playlist == null)
                    game.playlist = new List<string>() { playlist };
                else if (!game.playlist.Contains(playlist))
                    game.playlist.Add(playlist);
            }
            else
            {
                if ((game.playlist != null) && (game.playlist.Contains(playlist)))
                {
                    game.playlist.Remove(playlist);
                    if (game.playlist.Count == 0)
                        game.playlist = null;
                }
            }

            return await SetPlaylistGame(game);






        }

        public async Task<bool> UpdateVideogameRoms(VideoGameDb videogame)
        {
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var update = Builders<VideoGameDb>.Update.Set(g => g.roms, videogame.roms).Set(g => g.romscount, videogame.romscount);
            var r = await collection.UpdateOneAsync(g => g._id == videogame._id, update, new UpdateOptions { IsUpsert = false });

            return r.MatchedCount == 1;
        }

        public async Task<bool> InsertVideogameFull(VideoGameFullDb videogamefull)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<VideoGameFullDb>(_settings.collectionNameVideoGames);

            await collection.InsertOneAsync(videogamefull);
            return true;
        }

        public static string regexformat(string value)
        {
            var t = value.Replace("(", "\\(");
            t = t.Replace(")", "\\)");
            t = t.Replace("[", "\\[");
            t = t.Replace("]", "\\]");
            return t;

        }


        public async Task<VideoGameDb?> SearchVideoGameByRomId(string romId)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return null;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);

            var filter = Builders<VideoGameDb>.Filter.Empty;
            filter = filter & Builders<VideoGameDb>.Filter.ElemMatch<RomDb>(g => g.roms, Builders<RomDb>.Filter.Where(r => r.romid.ToLower() == romId.ToLower()));
            return await collection.Find(filter).Limit(1).FirstOrDefaultAsync();
        }
        public async Task<VideoGameDb?>  SearchVideoGameByRom (string systemId, string romName)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return null;
            }
            IMongoCollection<VideoGameDb> collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);

            var filter = Builders<VideoGameDb>.Filter.Empty;
            filter = filter & Builders<VideoGameDb>.Filter.Eq(g => g.systemid, systemId);
            filter = filter & Builders<VideoGameDb>.Filter.ElemMatch<RomDb>(g => g.roms, Builders<RomDb>.Filter.Where(r => r.name.ToLower()==romName.ToLower()));
            return await collection.Find(filter).Limit(1).FirstOrDefaultAsync();
        }

        public async Task<VideoGameDb?> RomActionRomForVideogame (string videogameid, string romid, RomAction romaction)
        {
            var videogame = await GetVideoGame(videogameid);
            if (videogame == null)
                return null;
            bool romfound = false;
            foreach (var rom in videogame.roms)
            {
                if (rom.romid == romid)
                {
                    var r = rom;
                    if (romaction != RomAction.LINK)
                        videogame.roms.Remove(r);
                    if (romaction == RomAction.SETPRIMARY)
                        videogame.roms.Insert(0, r);
                    romfound = true;
                    break;
                }
            }
            if (!romfound)
            {
                if (romaction == RomAction.LINK)
                {
                    var rom = await GetRom(romid);
                    if (rom==null)
                        return null;
                    videogame.roms.Add(Cast.ConvertRom(rom));
                }
                else
                    return null;
            }

            videogame.romscount = videogame.roms.Count;
            var updateresult = await UpdateVideogameRoms(videogame);
                if (!updateresult)
                    return null;
            
            if (romaction == RomAction.UNLINK || romaction == RomAction.LINK)
            {// remove match for rom
                var rom = await GetRom(romid);
                if (rom == null)
                    return null;
                switch (romaction)
                {
                    case RomAction.LINK:
                        rom.isMatch = true;
                        rom.scrapperResult = 202;
                        rom.parsingexception = string.Format("Link to {0}", videogameid);
                        if (rom.systemCategory == "Arcade")
                            rom.systemid = videogame.systemid;
                        break;
                    case RomAction.UNLINK:
                        rom.isMatch = false;
                        rom.scrapperResult = 409;
                        rom.parsingexception = string.Format("UnLink from {0}", videogameid);
                        if (rom.systemCategory == "Arcade")
                            rom.systemid = "";
                        break;
                }
                var updateromresult = await UpdateMatchRom(rom);
                if (!updateromresult)
                    return null;
            }           

            return videogame;

        }

        public async Task<VideoGameDb?> UpdateSettingsVideogame(VideoGameDb item)
        {
            var collectionname = _settings.collectionNameVideoGames;
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return null;
            }
            var collection = _database.GetCollection<VideoGameDb>(collectionname);

            // Update excluded : GamePAth, Extension
            var update = Builders<VideoGameDb>.Update
                .Set(s => s.name, item.name)
                .Set(s => s.collection, item.collection)
                .Set(s => s.year, item.year)
                .Set(s => s.collectionId, item.collectionId)
                .Set(s => s.editorname, item.editorname)
                .Set(s => s.developname, item.developname)
                .Set(s => s.rating, item.rating)
                .Set(s => s.nbplayers, item.nbplayers);
            try
            {
                var updateresult = await collection.UpdateOneAsync(s => s._id == item._id, update, new UpdateOptions { IsUpsert = false });
            }
            catch (Exception ex)
            {
                return null;
            }

            return await GetVideoGame(item._id);
        }

        public async Task<List<string>> GetAllPlaylist()
        {
            var result = new List<string>();
            var collection = _database.GetCollection<VideoGameDb>(CollectionName[EnumCollection.VIDEOGAME]);
            var filter = Builders<VideoGameDb>.Filter.Where(v => v.playlist != null);
            var vg = (await collection.FindAsync(filter)).ToList();
            foreach (var v in vg)
            { 
                foreach (var p in v.playlist)
                {
                    if (!result.Contains(p))
                    {
                        result.Add(p);
                    }
                }
            }
            return result;
        }

        #endregion

        #region ROM

        public async Task<GbdRomUpdateResult> UpdateRom(List<Rom> roms, IMongoCollection<Rom>? collection = null)
        {
            var result = new GbdRomUpdateResult();
            result.Initialize(roms.Count);

            if (collection == null)
                collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            foreach (Rom rom in roms)
            {
                // Excluded (_id, isMatch, firstscandate)
                var update = Builders<Rom>.Update
                    .Set(g => g.fullpath, rom.fullpath)
                    .Set(g => g.systemCategory, rom.systemCategory)
                    .Set(g => g.size, rom.size)
                    .Set(g => g.extension, rom.extension)
                    .Set(g => g.name, rom.name)
                    .Set(g => g.fullname, rom.fullname)
                    .Set(g => g.systemid, rom.systemid)
                    .Set(g => g.official, rom.official)
                    .Set(g => g.version, rom.version)
                    .Set(g => g.region, rom.region)
                    .Set(g => g.core, rom.core)
                    .Set(g => g.checksum_md5, rom.checksum_md5)
                    .Set(g => g.checksum_crc, rom.checksum_crc)                  
                    .Set(g => g.lastscandate, rom.lastscandate)
                    .Set(g => g.scrapperResult, rom.scrapperResult)
                    .Set(g => g.date, rom.date);

                var r = await collection.UpdateOneAsync(g => g._id == rom._id, update, new UpdateOptions { IsUpsert = false });
                if (r.MatchedCount == 0)
                {
                    result.IncInsert();
                    await collection.InsertOneAsync(rom);
                }
                else
                {
                    result.IncUpdate();
                }
               

               
            }
            return result;

        }

        public async Task<GbdRomUpdateResult> UpdateRom(Rom rom, IMongoCollection<Rom>? collection = null)
        {
            var result = new GbdRomUpdateResult();
            //result.Initialize(1);

            if (collection == null)
                collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);

            // Excluded (_id, isMatch, firstscandate)
            var update = Builders<Rom>.Update
                    .Set(g => g.fullpath, rom.fullpath)
                    .Set(g => g.systemCategory, rom.systemCategory)
                    .Set(g => g.size, rom.size)
                    .Set(g => g.extension, rom.extension)
                    .Set(g => g.name, rom.name)
                    .Set(g => g.fullname, rom.fullname)
                    .Set(g => g.systemid, rom.systemid)
                    .Set(g => g.official, rom.official)
                    .Set(g => g.version, rom.version)
                    .Set(g => g.region, rom.region)
                    .Set(g => g.core, rom.core)
                    .Set(g => g.checksum_md5, rom.checksum_md5)
                    .Set(g => g.checksum_crc, rom.checksum_crc)
                    .Set(g => g.lastscandate, rom.lastscandate)
                    .Set(g => g.scrapperResult, rom.scrapperResult)
                    .Set(g => g.date, rom.date);

            var r = await collection.UpdateOneAsync(g => g._id == rom._id, update, new UpdateOptions { IsUpsert = false });
            if (r.MatchedCount == 0)
            {
                result.IncInsert();
                await collection.InsertOneAsync(rom);
            }
            else
            {
                result.IncUpdate();
            }




            return result;

        }

        public async Task<bool> UpdateMatchRom(Rom rom)
        {
            var collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);

            // Excluded (_id, isMatch, firstscandate)
            var update = Builders<Rom>.Update
                .Set(g => g.isMatch, rom.isMatch)
                .Set(g => g.systemid, rom.systemid)
                .Set(g => g.region, rom.region)
                .Set(g => g.language, rom.language)
                .Set(g => g.supporttype, rom.supporttype)
                .Set(g => g.responsetime, rom.responsetime)
                .Set(g => g.scrapperResult, rom.scrapperResult)
                .Set(g => g.parsingexception, rom.parsingexception);

            var r = await collection.UpdateOneAsync(g => g._id == rom._id, update, new UpdateOptions { IsUpsert = false });
            return r.MatchedCount == 1;            
        }

        public async Task<List<Rom>> SelectUnmatchedRom (string systemId, string systemCategory,  List<int> resultcode, int limit = 0)
        {
            var collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            var filter = Builders<Rom>.Filter.Eq(r => r.isMatch, false);
            if (!string.IsNullOrEmpty(systemId))
            {
                filter = filter & Builders<Rom>.Filter.Eq(r => r.systemid, systemId);
            }
            if (!string.IsNullOrEmpty(systemCategory))
            {
                filter = filter & Builders<Rom>.Filter.Eq(r => r.systemCategory, systemCategory);
            }

            
            
            if (resultcode.Count>0)
            {
                var f = Builders<Rom>.Filter.Exists(g => g.scrapperResult, false);
                foreach (var code in resultcode)
                {
                    f = f | Builders<Rom>.Filter.Eq(g => g.scrapperResult, code);
                }
                filter = filter & f;
            }
            else
            {
                var f = Builders<Rom>.Filter.Exists(g => g.scrapperResult, false) | Builders<Rom>.Filter.Ne(g => g.scrapperResult, 200);
                filter = filter & f;
            }
            
            if (limit==0)
                return await collection.Find(filter).ToListAsync();
            else
                return await collection.Find(filter).Limit(limit).ToListAsync();

        }


        public async Task<Rom?> GetRom(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            IMongoCollection<Rom> collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            return await collection.Find(g => g._id == id).FirstOrDefaultAsync<Rom>();
        }

        public async Task<bool> DeleteRom (string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            var deleteresult = await collection.DeleteOneAsync(v => v._id == id);
            return deleteresult.DeletedCount == 1;
        }

        #endregion

        #region Generic
        private SortDefinition<T> GetSort<T>(List<SortDb> SortFields)
        {
            if (SortFields.Count == 0)
            {
                return Builders<T>.Sort.Ascending("year");
            }

            return Builders<T>.Sort.Combine(SortFields.Select(a => a.IsAscending ? Builders<T>.Sort.Ascending(a.Field) : Builders<T>.Sort.Descending(a.Field)));

        }
        public async Task<List<T>> GetDistinctValues<T>(EnumCollection ecollection, string property)
        {
            var filter = new BsonDocument();
            IMongoCollection<BsonDocument> collection = _database.GetCollection<BsonDocument>(CollectionName[ecollection]);
            return (await collection.DistinctAsync<T>(property, filter)).ToList();//  .ToList<string>();
        }

        private async Task<long> CountDocumentAsync<T>(EnumCollection ecollection, FilterDefinition<T>? filter = null) where T : class
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return 0;
            }
            var collection = _database.GetCollection<T>(CollectionName[ecollection]);
            if (filter==null)
            {
                filter = Builders<T>.Filter.Empty;
            }
            return await collection.CountDocumentsAsync(filter);
        }



        #endregion

        #region Media

        public async Task<MediaDb?> GetMediaDbBySource (string source)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return (MediaDb?)null;
            }
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);

            var media = await collection.Find(g => g.source == source).FirstOrDefaultAsync<MediaDb>();
            return media;
        }

        public async Task<MediaDb?> GetMediaDbById(string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return (MediaDb?)null;
            }
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);

            var media = await collection.Find(g => g._id == id).FirstOrDefaultAsync<MediaDb>();
            return media;
        }

        public async Task<bool> InserMedia (MediaDb media)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);

            await collection.InsertOneAsync(media);
            return true;
        }

        public async Task<bool> UpdateMedia (MediaDb media)
        {
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);

            var update = Builders<MediaDb>.Update
                .Set(g => g.filename, media.filename)
                .Set(g => g.targetpath, media.targetpath)
                .Set(g => g.size, media.size)
                .Set(g => g.extension, media.extension)
                .Set(g => g.contenttype, media.contenttype);

            var r = await collection.UpdateOneAsync(g => g._id == media._id, update, new UpdateOptions { IsUpsert = false });
            return r.MatchedCount > 0;
        }

        public async Task<bool> DeleteMedia(string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);
            var deleteresult = await collection.DeleteOneAsync(v => v._id == id);
            return deleteresult.DeletedCount==1;
        }

        public long MediaDownloadSize ()
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return 0;
            }
            var collection = _database.GetCollection<MediaDb>(_settings.collectionNameMedia);

            return collection.AsQueryable().Where(s => s.size>0).Sum(s => s.size);
          
        }

        #endregion

        #region CoreSaveState
        public async Task<CoreSaveState?> GetCoreSaveState (string id)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return (CoreSaveState?)null;
            }
            IMongoCollection<CoreSaveState> collection = _database.GetCollection<CoreSaveState>(_settings.collectionNameCoreSaveState);
            var item = await collection.Find(g => g._id == id).FirstOrDefaultAsync<CoreSaveState>();
            return item;
        }
        public async Task<List<CoreSaveState>> GetCoreSaveState(string videogameid, string romid)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return new List<CoreSaveState>();
            }
            IMongoCollection<CoreSaveState> collection = _database.GetCollection<CoreSaveState>(_settings.collectionNameCoreSaveState);
            var filter = Builders<CoreSaveState>.Filter.Eq(s => s.videogameid, videogameid)
                & Builders<CoreSaveState>.Filter.Eq(s => s.romid, romid);
            var sort = Builders<CoreSaveState>.Sort.Ascending("slot");
            var item = await collection.Find(filter).Sort(sort).ToListAsync();
            return item;
        }

        public async Task<bool?> UpdateCoreSaveState(CoreSaveState savestate)
        {
            var collection = _database.GetCollection<CoreSaveState>(_settings.collectionNameCoreSaveState);
            var update = Builders<CoreSaveState>.Update
                .Set(g => g.mediaId, savestate.mediaId)
                .Set(g => g.filename, savestate.filename)
                .Set(g => g.Modified, savestate.Modified);

            var r = await collection.UpdateOneAsync(g => g._id == savestate._id, update, new UpdateOptions { IsUpsert = false });
            if (r.MatchedCount == 0)
            {
                await collection.InsertOneAsync(savestate);
            }
            return true;
        }

        #endregion

        #region Statistics

        public async Task<MisterStats> GetStats()
        {
            var result = new MisterStats();
            result.systemsCount = await CountDocumentAsync<SystemDb>(EnumCollection.SYSTEM);
            result.systemsCountWithVideogames = await CountDocumentAsync(EnumCollection.SYSTEM, Builders<SystemDb>.Filter.Gt(s => s.statvideogame, 0));
            result.videogamesCount = await CountDocumentAsync<VideoGameDb>(EnumCollection.VIDEOGAME);
            result.romsCount = await CountDocumentAsync<Rom>(EnumCollection.ROM);
            result.romsCountMatch= await CountDocumentAsync<Rom>(EnumCollection.ROM, Builders<Rom>.Filter.Eq(s => s.isMatch, true));
            result.mediaCount = await CountDocumentAsync<MediaDb>(EnumCollection.MEDIA);
            result.mediaDownloadCount = await CountDocumentAsync<MediaDb>(EnumCollection.MEDIA, Builders<MediaDb>.Filter.Gt(m => m.size, 0));
            result.mediaDownloadSize = MediaDownloadSize();
            return result;
        }

        public void videogamestatistics ()
        {
            IMongoCollection<VideoGameDb> collectionVideogame = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            IMongoCollection<Rom> collectionRom = _database.GetCollection<Rom>(_settings.collectionNameRoms);

            var systemdictionary = new Dictionary<string, systemstat>();

            // CALCULTATE VIDEOGAME
            var matchFilter = Builders<VideoGameDb>.Filter.Gt(r => r.romscount, 0);
            // Defines the aggregation pipeline with the $match and $group aggregation stages
            var pipeline = new EmptyPipelineDefinition<VideoGameDb>()
                .Match(matchFilter)
                .Group(r => r.systemid,
                    g => new
                    {
                        _id = g.Key,
                        Count = g.Count()
                    }
                );
            var cursor = collectionVideogame.Aggregate(pipeline).ToList();
            foreach (var c in cursor)
            {
                if (systemdictionary.ContainsKey(c._id))
                    systemdictionary[c._id].videogame = c.Count;
                else
                    systemdictionary.Add(c._id, new systemstat() {  videogame = c.Count });

                
            }

            // CALCULATE ROMS FOUND
            var matchromFilter = Builders<Rom>.Filter.Empty;
            // Defines the aggregation pipeline with the $match and $group aggregation stages
            var pipelineroms = new EmptyPipelineDefinition<Rom>()
                .Match(matchromFilter)
                .Group(r => r.systemid,
                    g => new
                    {
                        _id = g.Key,
                        Count = g.Count()
                    }
                );
            var cursorRom = collectionRom.Aggregate(pipelineroms).ToList();
            foreach (var c in cursorRom)
            {
                if (systemdictionary.ContainsKey(c._id))
                    systemdictionary[c._id].romfound = c.Count;
                else
                    systemdictionary.Add(c._id, new systemstat() { romfound = c.Count });


            }

            // CALCULATE ROMS Match FOUND
            var matchrommatchFilter = Builders<Rom>.Filter.Eq ( r => r.isMatch, true);
            // Defines the aggregation pipeline with the $match and $group aggregation stages
            var pipelineromsmatch = new EmptyPipelineDefinition<Rom>()
                .Match(matchrommatchFilter)
                .Group(r => r.systemid,
                    g => new
                    {
                        _id = g.Key,
                        Count = g.Count()
                    }
                );
            var cursorRomMatch = collectionRom.Aggregate(pipelineromsmatch).ToList();
            foreach (var c in cursorRomMatch)
            {
                if (systemdictionary.ContainsKey(c._id))
                    systemdictionary[c._id].rommatch = c.Count;
                else
                    systemdictionary.Add(c._id, new systemstat() { rommatch = c.Count });


            }



            foreach (var s in GetAllSystems().Result.Select(s => s._id))
            {
                systemstat stat = systemdictionary.ContainsKey(s) ? systemdictionary[s] : new systemstat();
                var updateresult = UpdateSystemStatistics(s, stat.videogame, stat.romfound, stat.rommatch).Result;
            }
            



        }
        #endregion

        #region patch value

        public void PatchVideoGameDate ()
        {
            var preferedregion = new List<string>() { "fr", "eu", "ss", "wor", "us", "jp" };
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var filter = Builders<VideoGameDb>.Filter.Lt(v => v.year, 2);

            var patchitem = collection.Find(filter).ToList();
            foreach (var item in patchitem.Where(v => v.dates != null && v.dates.Count>0))
            {
                var foundprefereddate = false;
                var datesearch = "";
                var regionsearch = "";
                int i = 0;
                while (!foundprefereddate && i < preferedregion.Count)
                {

                    foreach (var regionvalue in item.dates)
                    {
                        if (!foundprefereddate && regionvalue.regions.Contains(preferedregion[i]))
                        {
                            foundprefereddate = true;
                            datesearch = regionvalue.value;
                            regionsearch = preferedregion[i];
                        }
                    }
                    i++;
                }
                if (!foundprefereddate)
                {
                    datesearch = item.dates[0].value;
                    regionsearch = item.dates[0].regions[0];
                }
               
                    var gamedate = DateTime.MinValue;
                    if (Regex.IsMatch(datesearch, @"^\d{2}$"))
                    {
                        gamedate = new DateTime(1900 + int.Parse(datesearch), 1, 1);
                    }
                    else if (Regex.IsMatch(datesearch, @"^\d{4}$"))
                    {
                        gamedate = new DateTime(int.Parse(datesearch), 1, 1);
                    }
                    else if (Regex.IsMatch(datesearch, @"^\d{4}-\d{2}$"))
                    {
                        bool v1 = DateTime.TryParseExact(datesearch, "yyyy-MM", (CultureInfo)CultureInfo.InvariantCulture.Clone(), DateTimeStyles.None, out gamedate);
                    }
                    else
                    {
                        bool v = DateTime.TryParseExact(datesearch, "yyyy-MM-dd", (CultureInfo)CultureInfo.InvariantCulture.Clone(), DateTimeStyles.None, out gamedate);
                    }

                    if (gamedate != DateTime.MinValue)
                    {
                        item.date_region = regionsearch;
                        item.gamedate = gamedate.AddHours(2);
                        item.year = gamedate.Year;
                   
                    // update item



                    var update = Builders<VideoGameDb>.Update.Set(v => v.gamedate, item.gamedate)
                        .Set(v => v.date_region, item.date_region)
                        .Set(v => v.year, item.year);

                    var r =  collection.UpdateOne(g => g._id == item._id, update, new UpdateOptions { IsUpsert = false });
                }
                
            }
            // select dates issues

        }

        public void PatchArcadeVideoGameRomCore()
        {
            var preferedregion = new List<string>() { "fr", "eu", "ss", "wor", "us", "jp" };
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var filter = Builders<VideoGameDb>.Filter.Eq(v => v.systemcategory, "Arcade")
                & Builders<VideoGameDb>.Filter.Gt(v => v.romscount, 1);

            var patchitem = collection.Find(filter).ToList();
            foreach (var item in patchitem)
            {
                foreach (var rom in item.roms.Where(r => string.IsNullOrEmpty(r.core)))
                {
                    var parentrom = GetRom(rom.romid).Result;
                    if (parentrom!=null)
                    {
                        rom.core = parentrom.core;
                    }
                }




                var update = Builders<VideoGameDb>.Update.Set(v => v.roms, item.roms);                   

                var r = collection.UpdateOne(g => g._id == item._id, update, new UpdateOptions { IsUpsert = false });
                
            }
            // select dates issues

        }

        public void PatchSystemEndYear()
        {
            var collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);
            var filter = Builders<SystemDb>.Filter.Gte(v => v.enddate, DateTime.MinValue);

            var patchitem = collection.Find(filter).ToList();
            foreach (var item in patchitem)
            {
                item.endyear = item.enddate.Year;




                var update = Builders<SystemDb>.Update.Set(v => v.endyear, item.endyear);

                var r = collection.UpdateOne(g => g._id == item._id, update, new UpdateOptions { IsUpsert = false });

            }
            // select dates issues

        }


        public void PatchUdpateSystemExtension()
        {
            var collection = _database.GetCollection<SystemDb>(_settings.collectionNameSystems);
            var filter = Builders<SystemDb>.Filter.Eq(v => v._id, "TurboGrafx16");

            var patchitem = collection.Find(filter).ToList();
            foreach (var item in patchitem)
            {
                item.extensions = item.extensions + ",zip";




                var update = Builders<SystemDb>.Update.Set(v => v.extensions, item.extensions);

                var r = collection.UpdateOne(g => g._id == item._id, update, new UpdateOptions { IsUpsert = false });

            }
            // select dates issues

        }

        public void PatchUpdateRomValueFromScrapping()
        {
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var filter = Builders<VideoGameDb>.Filter.Empty;

            var patchitem = collection.Find(filter).ToList();
            foreach (var item in patchitem)
            {
                foreach (var rom in item.roms)
                {
                    var r = GetRom(rom.romid).Result;
                    if (r != null)
                    {
                        if (string.IsNullOrEmpty(r.region))
                            r.region = rom.region;
                        r.language = rom.language;
                        r.supporttype = rom.supportType;

                        var result = UpdateMatchRom(r).Result;
                    }

                }
            }
        }        

        public void PatchArcadeRomSystemId()
        {
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var collectionrom = _database.GetCollection<Rom>(_settings.collectionNameRoms);
            var filter = Builders<VideoGameDb>.Filter.Eq(v => v.systemcategory, "Arcade");
            var videogames = collection.Find(filter).ToList();
            foreach (var v in videogames)
            {
                if (v.roms != null)
                {
                    foreach (var romid in (v.roms.Select(r => r.romid)))
                    {
                        var update = Builders<Rom>.Update.Set(r => r.systemid, v.systemid);
                        collectionrom.UpdateOne(r => r._id == romid, update, new UpdateOptions() { IsUpsert = false });
                    }
                }
            }

        }

        public string PatchArcadeVideogameWithoutCore()
        {
            var collection = _database.GetCollection<VideoGameDb>(_settings.collectionNameVideoGames);
            var filter = Builders<VideoGameDb>.Filter.Eq(v => v.systemcategory, "Arcade");
            var videogames = collection.Find(filter).ToList();

            var report = new StringBuilder();
            report.AppendFormat("{0} Videogames found\r\n", videogames.Count);
            int fix = 0;

            foreach (var v in videogames)
            {
                if (v.roms != null)
                {
                    var updatenecessary = false;
                    foreach (var rom in v.roms)
                    {
                        if (string.IsNullOrEmpty(rom.core))
                        {
                            report.AppendFormat("Videogame {0} - Rom {1} is empty\r\n", v.name, rom.romid);
                            var romdb = GetRom(rom.romid).Result;
                            if (romdb!=null)
                            {
                                report.AppendFormat("\tRom found with core {0}\r\n",  romdb.core);
                                rom.core = romdb.core;
                                updatenecessary = true;
                                fix++;
                            }
                            else
                            {
                                report.AppendFormat("\tError rom doesnot exist...");
                            }
                        }
                    }
                    if (updatenecessary)
                    {
                        var updatesucced = UpdateVideogameRoms(v).Result;
                        report.AppendFormat("Update Videogame {0} result : {1}", v.name, updatesucced ? "succed" : "failed");

                    }
                }
            }
            report.AppendFormat("{0} Fix", fix);

            return report.ToString();

        }


        #endregion


        #region ModuleSettings
        public async Task<List<ModuleSetting>> GetModuleSettings(string moduleName)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return new List<ModuleSetting>();
            }
            var collection = _database.GetCollection<ModuleSetting>(_settings.collectionNameModuleSettings);
            return await collection.Find(g => g.moduleName == moduleName).ToListAsync();
        }

        public async Task<bool> SetModuleSettings(List<ModuleSetting> moduleSettings)
        {
            if (_health.MisterState != MisterStateEnum.OK)
            {
                return false;
            }
            var collection = _database.GetCollection<ModuleSetting>(_settings.collectionNameModuleSettings);
            foreach (var moduleSetting in moduleSettings)
            {
                var update = Builders<ModuleSetting>.Update
                    .Set(m => m.value, moduleSetting.value)
                    .Set(m => m.valueType, moduleSetting.valueType)
                    .Set(m => m.update, moduleSetting.update)
                    .Set(m => m.description, moduleSetting.description);

                var updatecount = await collection.UpdateOneAsync(m => m._id == moduleSetting._id, update);
                if (updatecount.MatchedCount == 0)
                {
                    await collection.InsertOneAsync(moduleSetting);
                }
            }
            return true;
        }

    }
        #endregion


    internal class systemstat
    {
        public int videogame { get; set; } = 0;
        public int romfound { get; set; } = 0;
        public int rommatch { get; set; } = 0;

    }

    


}