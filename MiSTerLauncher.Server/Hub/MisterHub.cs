using Amazon.Runtime.Internal.Util;
using libMisterLauncher.Entity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using MiSTerLauncher.Server.HostedService;

namespace MiSTerLauncher.Server.Hub
{

    public class MisterHub : Hub<IMisterHubClient>
    {
        private MisterBackgroundTask _hostedservice;
        private readonly ILogger<MisterBackgroundTask> _logger;
        public MisterHub(MisterBackgroundTask hostedService, ILogger<MisterBackgroundTask> logger)
        {
            _hostedservice = hostedService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(string.Format("[SignalR] New connection with id : {0}", Context.ConnectionId));
            //await Clients.All.RefreshCache(_hostedservice.manager.Cache());
            await Clients.Client(Context.ConnectionId).RefreshCache(_hostedservice.manager.Cache());
            
            //await Clients.All.SendMessage("New connection");//.SendAsync("ReceiveMessage", $"Received the message: Another connection has been added.");
        }

        public async Task RefreshCache()
        {
            _logger.LogInformation("[SignalR] Send Cache Refresh");
            await Clients.All.RefreshCache(_hostedservice.manager.Cache());
        }       

    }
}


public interface IMisterHubClient
{
    Task JobRomScanRefresh(JobMister cache);
    Task RefreshCache(MisterManagerCache cache);
    Task SendMessage(string message);

    //Results RefreshCache(MisterManagerCache cache);
}