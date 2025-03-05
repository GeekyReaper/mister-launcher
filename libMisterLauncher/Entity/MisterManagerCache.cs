using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libMisterLauncher.Entity
{
    public class MisterManagerCache
    {
        public MisterHealthCheck Health { get; } =  new MisterHealthCheck();
        public MisterStats Stats { get; set; } = new MisterStats();

        public MisterPlayingGame playingVideoGame { get; } = new MisterPlayingGame();

        public DateTime LastUpdate { get; set; }

        private Dictionary<string, SystemDb> _systems = new Dictionary<string, SystemDb>();   

        public void StoreSystems(SystemDb system)
        {
            if (!_systems.ContainsKey(system._id))                
            {
                _systems.Add(system._id, system);
            }
            else
                _systems[system._id] = system;
        }
        public SystemDb? GetSystem(string systemid)
        {
            if (_systems.ContainsKey(systemid))
            {
                return _systems[systemid];
            }
            return null;
        }

        public IEnumerable<SystemDb> GetSystems()
        {
            foreach (var item in _systems)
            {
                yield return item.Value;
            }
        }
    }


}
