using libMisterLauncher.Entity;
using libMisterLauncher.Manager;
using libMisterLauncher.Service;
using Microsoft.AspNetCore.SignalR;
using MiSTerLauncher.Server.Hub;

namespace MiSTerLauncher.Server.HostedService
{
    public class MisterBackgroundTask : IHostedService, IDisposable
    {
        private readonly IHubContext<MisterHub, IMisterHubClient> _hub;
        private int executionCount = 0;
        private readonly ILogger<MisterBackgroundTask> _logger;
        private Timer? _timer = null;
        private bool _sleepmode = false;
        private DateTime _lastactivity = DateTime.Now;
        private MisterManager _manager;
        public MisterManager manager { get {               
                return _manager; } }

        public MisterBackgroundTask(ILogger<MisterBackgroundTask> logger, IHubContext<MisterHub, IMisterHubClient> hub)
        {
            _logger = logger;
            _hub = hub;
        }
        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Wakeup ()
        {
            _lastactivity = DateTime.Now;
            if (_sleepmode)
            {
                _sleepmode = false;
                _logger.LogInformation("Exist sleep mode");
                ForceRefresh();
            }
        }

        public void ForceRefresh(int delay_ms=0)
        {
            Task.Run(() => {
                // check if module is alive
                _manager.checkModuleList();
                // Temporise
                if (delay_ms > 0)
                    Thread.Sleep(delay_ms);
                // Activate works
                DoWork(null);
                });
        }


        private GameDbSettings GetgamedbSettings()
        {
            var result = new GameDbSettings();
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GDB_MONGO_CNX")))
            {
                _logger.LogCritical("No Mongo CNX. Please set environment varaible [GDB_MONGO_CNX]");
            }
            else
            {
                result.connectionstring = Environment.GetEnvironmentVariable("GDB_MONGO_CNX");
            }
            
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GDB_MONGO_DBNAME")))
            {
                result.dbname = Environment.GetEnvironmentVariable("GDB_MONGO_DBNAME");
            }

            return result;
        }        

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MisterBackgroundTask Service running.");

            _manager = new MisterManager();
            _manager.OnCacheUpdated += _manager_OnCacheUpdated;
            _manager.OnJobRomUpdate += _manager_OnJobRomUpdate;           

            _manager.Initialize(GetgamedbSettings());
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(30));

            return Task.CompletedTask;
        }

        private void _manager_OnJobRomUpdate(JobMister job)
        {
            _hub.Clients.All.JobRomScanRefresh(job);
        }

        private void _manager_OnCacheUpdated()
        {
            _hub.Clients.All.RefreshCache(_manager.Cache());
        }

        private void DoWork(object? state)
        {
            if (!_sleepmode)
            {
                var lastactivitytime = DateTime.Now - _lastactivity;
                if (lastactivitytime.TotalMinutes > 20)
                {
                    _logger.LogInformation(
                    string.Format("Enter Sleep mode, no activity since {0} min", lastactivitytime.TotalMinutes));
                    _sleepmode = true;
                }
                    _manager.RefreshCache();
                _logger.LogInformation(
                    "MisterBackgroundTask RefreshCache");
                
                _hub.Clients.All.RefreshCache(_manager.Cache());
            }
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("MisterBackgroundTask Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
