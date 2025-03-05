using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using ZstdSharp.Unsafe;
using static System.Net.WebRequestMethods;
using ThirdParty.Json.LitJson;
using System.Text.Json;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Net.WebSockets;
using System.Web;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("MisterLauncher.Test")]
namespace libMisterLauncher.Service
{

    public delegate void GamePlaying (RemoteServiceCurrentGame currentGame);
    
    public class RemoteServiceSettings : IMisterSettings
    {
        const string _moduleName = "MisterRemote";

        public string ModuleName { get { return _moduleName; } }
        public string host { get; set; } = "";

       
       
        public bool isValid()
        {
            return !string.IsNullOrEmpty(host);
        }       

        public List<ModuleSetting> GetModuleSettings()
        {
            var result = new List<ModuleSetting>
            {
                new ModuleSetting()
                {
                    moduleName = _moduleName,
                    name = "host",
                    value = host,
                    valueType = "url",
                    description = "default MiSTer value 'http://{misterip|misterlocalname}:8182'",
                    update = DateTime.Now
                }
            };
            foreach (var item in result)
                item.SetId();
            return result;
        }
        public void LoadModuleSettings(List<ModuleSetting> moduleSettings)
        {
            foreach (var moduleSetting in moduleSettings)
            {
                if (moduleSetting.name == "host") { host = moduleSetting.value; };
            }
        }
    }

    internal class MisterRemoteService : BaseModule<RemoteServiceSettings>
    {      

        public event GamePlaying? OnGamePlaying;

        internal ClientWebSocket? clientWebSocket = null;
        internal Task? clientWebSocketTask = null;

        DateTime _lastPlayingGameSend = DateTime.MinValue;
        RemoteServiceCurrentGame _lastCurrentGame;

         List<TaskStatus> _taskStopper = new() { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };

        internal bool websocketIsOperational
        {
            get
            {
                return clientWebSocket != null && clientWebSocketTask != null && !_taskStopper.Contains(clientWebSocketTask.Status);
            }
        }

        public MisterRemoteService() : base()
        {
        }
        public MisterRemoteService(RemoteServiceSettings settings) : base(settings)
        {
        }

        


        public GameAction GetGameLaunchCommand(Game g, SystemDb? system)
        {
            var result = new GameAction()
            {
                Name = g.romname,
                Category = "Game"
            };

            if ((system != null) && (g.fullpath.EndsWith(".zip") && !string.IsNullOrEmpty(system.extensions)))
            { // Complete ZIP
                result.Parameters.Add("Path", g.fullpath + "/" + g.fullpath.Split("/").Last().Replace(".zip", ".") + system.extensions.Split(',').First());
            }
            else
               result.Parameters.Add("Path", g.fullpath);


            return result;
        }

        public override bool CheckConnection()
        {           
            var systems = GetRequest("api/systems").Result;
            var result = systems != null && systems.IsValid();
            if (result && !websocketIsOperational)
            { // Open and listen WebSocket                
                    clientWebSocketTask = ConnectAndListenToRemoteWebSocket(new Uri(string.Format("{0}/api/ws", _settings.host.Replace("http://", "ws://"))));                
            }
            if (!result)
            {
                if (OnGamePlaying != null)
                {
                    OnGamePlaying(new RemoteServiceCurrentGame());
                }
            }

            return result;
           
        }

        #region WebSocket REMOTE
        public async Task ConnectAndListenToRemoteWebSocket(Uri serverUri)
        {
            if (clientWebSocket!=null)
            {
                clientWebSocket.Dispose();
            }
            clientWebSocket = new ClientWebSocket();
            await clientWebSocket.ConnectAsync(serverUri, CancellationToken.None);

            Console.WriteLine("Connected to the server. Start sending messages...");            

            while (clientWebSocket.State == WebSocketState.Open)
            {
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    Console.WriteLine($"[REMOTE-WEBSOCKET] Received message from MiSTer: {receivedMessage}");
                    if (OnGamePlaying != null)
                    {
                        var currentgame = extractCurrentgameInfo(receivedMessage);
                        if (currentgame != null)
                        {
                            var condignore = _lastCurrentGame!=null && currentgame.IsEmpty() && !_lastCurrentGame.IsEmpty() && ((DateTime.Now - _lastPlayingGameSend).TotalSeconds < 2);
                            if (condignore)
                                Console.WriteLine($"[REMOTE-WEBSOCKET] ignore message from MiSTer: {receivedMessage}");
                            if (!condignore)
                            {
                                OnGamePlaying(currentgame);
                                _lastCurrentGame = currentgame;
                                _lastPlayingGameSend = DateTime.Now;
                            }
                           
                        }
                    }
                }
            }
        }
        internal void SendCommandsToremoteWebSocket(List<string> cmds, int delay = 100)
        {
            try
            {
                if (!websocketIsOperational)
                    return;
                
                foreach (var msg in cmds)
                {
                        ArraySegment<byte> sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                        clientWebSocket.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                        Thread.Sleep(delay);                        
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[RemoteService] SendCommandsToremoteWebSocket : exception {0}", ex.Message));
            }
        }

        internal async Task ListenRemoteWebSocket()
        {
            while (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                byte[] receiveBuffer = new byte[1024];
                WebSocketReceiveResult result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string receivedMessage = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                    Console.WriteLine($"[REMOTE-WEBSOCKET]Received message from server: {receivedMessage}");
                    if (OnGamePlaying != null)
                    {
                        var currentgame = extractCurrentgameInfo(receivedMessage);
                        if (currentgame != null) 
                            OnGamePlaying(currentgame);
                    }
                }
            }
        }

        internal RemoteServiceCurrentGame? extractCurrentgameInfo (string message)
        {
            // Filter message
            if (!message.StartsWith("gameRunning") && !message.StartsWith("coreRunning"))
            {                
                return null;
            }
            var match = Regex.Match(message, @"gameRunning:(?<systemname>.+)\/(?<gamename>.*)\.(?<extension>\w+)");
            if (match.Success)
            {
                return new RemoteServiceCurrentGame()
                {
                    systemName = match.Groups["systemname"].Value,
                    system = match.Groups["systemname"].Value,
                    core = match.Groups["systemname"].Value.ToUpper(),
                    gameName = match.Groups["gamename"].Value,
                    game = message.Split(":")[1]
                };
            }
            match = Regex.Match(message, @"coreRunning:(?<corename>.+)");
            if (match.Success)
            {
                return new RemoteServiceCurrentGame()
                {
                    core = match.Groups["corename"].Value                   
                };
            }

            return new RemoteServiceCurrentGame();
        }
        #endregion



        internal async Task<JsonRequestResult> GetRequest(string action, string parameters = "")
        {
            var result = new JsonRequestResult();
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_settings.host);
                    client.DefaultRequestHeaders.Add("User-Agent", "MisterLauncher");
                    client.Timeout = new TimeSpan(0, 0, 5);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    var response = await client.GetAsync(action + (string.IsNullOrEmpty(parameters) ? "" : "?" + parameters));
                    watch.Stop();
                    result.responsetime = (int)watch.Elapsed.TotalMilliseconds;
                    result.HttpCode = (int)response.StatusCode;
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        return result;
                    var jsonData = await response.Content.ReadAsStringAsync();
                   
                        result.json = JsonNode.Parse(jsonData);
                }
                catch (Exception ex)
                {
                    result.parsingException = ex.Message;
                }
            }

            return result;
        }

        public async Task<bool> LaunchGame(string path, SystemDb? systemdb = null)
        {
            if (_health.MisterState != MisterStateEnum.OK) return false;
           

            if ((systemdb != null) && (path.EndsWith(".zip") && !string.IsNullOrEmpty(systemdb.extensions)))
            { // Complete ZIP
                path =  path + "/" + path.Split("/").Last().Replace(".zip", ".") + systemdb.extensions.Split(',').First();
            
            }
            var httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri(_settings.host);

            var request = new HttpRequestMessage(new HttpMethod("POST"), "api/games/launch");
            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            request.Content = new StringContent("{\"Path\" : \"" + path + "\"}",
                                    Encoding.UTF8,
                                    "application/json");

            var response = await httpclient.SendAsync(request);
            var result = response.StatusCode == HttpStatusCode.OK;
            return (response.StatusCode == HttpStatusCode.OK);
        }



        public async Task<bool> KeyboardCommand(List<string>cmds, bool raw, int delay)
        {
            if (_health.MisterState != MisterStateEnum.OK) return false;
            bool result = true;
            for (int i=0; i<cmds.Count; i++)
            {
                var httpclient = new HttpClient();
                httpclient.BaseAddress = new Uri(_settings.host);

                var request = new HttpRequestMessage(new HttpMethod("POST"), string.Format("{0}/{1}", raw ? "api/controls/keyboard-raw" : "api/controls/keyboard", cmds[i]));
                request.Headers.TryAddWithoutValidation("content-type", "application/json");
                var response = await httpclient.SendAsync(request);
                result = result &&  response.StatusCode == HttpStatusCode.OK;
                if (cmds.Count > 0 && i < cmds.Count - 1)
                {
                    Thread.Sleep(delay);
                }
            }
            return result;
        }

        public async Task<RemoteServiceCurrentGame?> CurrentGame()
        {
            return await GetJsonDeserialize<RemoteServiceCurrentGame>("api/games/playing");            
        }

        public async Task<List<RemoteServiceMenuItem>> GetArcadeMrafile()
        {
            return await GetMenuItem("/media/fat/_Arcade");
        }

        public async Task<RemoteServiceScriptResult> GetScript()
        {
            var result = await GetJsonDeserialize<RemoteServiceScriptResult>("api/scripts/list");
            return result;
        }

        public async Task<bool> ExecuteScript(string filename)
        {
            if (_health.MisterState != MisterStateEnum.OK) return false;


            var httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri(_settings.host);

            var request = new HttpRequestMessage(new HttpMethod("POST"), "api/scripts/launch/" + filename);
            request.Headers.TryAddWithoutValidation("content-type", "application/json");            

            var response = await httpclient.SendAsync(request);
            var result = response.StatusCode == HttpStatusCode.OK;
            return (response.StatusCode == HttpStatusCode.OK);
        }

        public async Task<List<RemoteServiceSystem>> GetSystems()
        {
            return await GetJsonDeserialize<List<RemoteServiceSystem>>("api/systems");           
        }

        internal async Task<T> GetJsonDeserialize<T>(string getparameters) where T : new()
        {
            var item = new T();
            if (_health.MisterState != MisterStateEnum.OK) return item;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_settings.host);
                client.DefaultRequestHeaders.Add("User-Agent", "MisterLauncher");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(getparameters);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    item = JsonSerializer.Deserialize<T>(jsonData);
                }
            }
            return item;
        }
        internal async Task<T> PostJsonDeserialize<T>(string action, StringContent content) where T : new()
        {
            var result = new T();
            if (_health.MisterState != MisterStateEnum.OK) return result;
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(new HttpMethod("POST"), string.Format("{0}/{1}", _settings.host, action));
                request.Headers.TryAddWithoutValidation("content-type", "application/json");
                request.Content = content;
                var response = await client.SendAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();//  .ReadAsAsync<RemoteServiceCurrentGame>().Result;
                    result = JsonSerializer.Deserialize<T>(jsonData);
                }
            }
            return result == null ? new T() : result;
        }

        internal async Task<List<RemoteServiceMenuItem>> GetMenuItem(string path)
        {
            //var result = new List<RemoteServiceMenuItem>();
            var itemlist = await PostJsonDeserialize<RemoteServiceMenuListItem>("api/menu/view", new StringContent("{\"path\" : \"" + path + "\"}",
                                        Encoding.UTF8,
                                        "application/json"));
            return itemlist.items;
        }

        public void CmdSaveState(int state)
        {
            websocket(new List<string>() { "kbdRawDown:56", string.Format("kbdRawDown:{0}", 58 + state), string.Format("kbdRawUp:{0}", 58 + state), "kbdRawUp:56" }, 100);
        }

        public void CmdLoadState(int state)
        {
            websocket(new List<string>() { string.Format("kbdRawDown:{0}", 58 + state), string.Format("kbdRawUp:{0}", 58 + state) }, 100);
        }

        internal void websocket (List<string> cmds, int delay=100)
        {
            SendCommandsToremoteWebSocket(cmds, delay);
            return;
            try
            {
                using (var ws = new ClientWebSocket())
                {
                    ws.ConnectAsync(new Uri("ws://192.168.1.100:8182/api/ws"), CancellationToken.None).Wait();

                    // var msgs = new List<string> { "kbdRawDown:56", "kbdRawDown:59", "kbdRawUp:59", "kbdRawUp:56" };
                    foreach (var msg in cmds)
                    {
                        ArraySegment<byte> sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                        ws.SendAsync(sendBuffer, WebSocketMessageType.Binary, true, CancellationToken.None).Wait();
                        Thread.Sleep(delay);
                    }



                    Console.Write("Closing ...");
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
                    Console.WriteLine("OK");


                }
            }
            catch (Exception)
            {
            }
        }

        #region Screenshots
        public async Task<List<RemoteScreenshotsInfo>> GetAllScreenshots()
        {
            return await GetJsonDeserialize<List<RemoteScreenshotsInfo>>("api/screenshots");
        }

        public async Task<bool> TakeScreenshot()
        {
            if (_health.MisterState != MisterStateEnum.OK) return false;
            bool result = true;
           
            var httpclient = new HttpClient();
            httpclient.BaseAddress = new Uri(_settings.host);

            var request = new HttpRequestMessage(new HttpMethod("POST"), "api/screenshots");
            request.Headers.TryAddWithoutValidation("content-type", "application/json");
            var response = await httpclient.SendAsync(request);
            result = result && response.StatusCode == HttpStatusCode.OK;
            return result;
        }

        public string ScreenshootUrl(RemoteScreenshotsInfo sc)
        {
            return string.Format("{0}/api/screenshots/{1}/{2}", _settings.host, sc.core, sc.filename);
        }

        public async Task<bool> DeleteScreenshot (RemoteScreenshotsInfo sc)
        {
            if (_health.MisterState != MisterStateEnum.OK) return false;
            bool result = true;

            var httpclient = new HttpClient();
            //httpclient.BaseAddress = new Uri(_settings.host);
            var response = await httpclient.DeleteAsync(ScreenshootUrl(sc));

            //var request = new HttpRequestMessage(new HttpMethod("DELETE"), string.Format("/api/screenshots/{1}/{2}", sc.core, sc.filename));
            //var response = await httpclient.SendAsync(request);
            result = result && response.StatusCode == HttpStatusCode.OK;
            return result;
        }
    
        #endregion

    }
}
