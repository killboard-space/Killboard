using Killboard.Data.Models;
using Killboard.Domain.DTO.Universe.System;
using Killboard.Domain.Interfaces;
using Killboard.Domain.Params;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Killboard.Domain.Repositories
{
    public class SystemRepository : ISystemRepository
    {
        private Dictionary<int, List<int>> _systemDestinations;

        private readonly KillboardContext _ctx;
        public SystemRepository(KillboardContext ctx)
        {
            _ctx = ctx;
        }

        public (IEnumerable<GetSystem> systems, int pages) GetAll(GetAllSystemParameters parameters) => (_ctx.systems
            .Select(s => new GetSystem
            {
                Name = s.name,
                Security = s.security_status,
                SystemID = s.system_id,
                ConstellationID = s.constellation_id
            })
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize), (int)Math.Ceiling(_ctx.systems.Count() / (double)parameters.PageSize));

        public GetSystem GetSystem(int systemId) => _ctx.systems.Where(s => s.system_id == systemId).Select(s => new GetSystem
        {
            Name = s.name,
            Security = s.security_status,
            SystemID = s.system_id,
            ConstellationID = s.constellation_id
        }).FirstOrDefault();

        public List<int> GetRoute(int fromSystem, int toSystem)
        {
            if (!_ctx.systems.Any(a => a.system_id == fromSystem)) throw new ApplicationException("Initial system invalid!");

            if (!_ctx.systems.Any(a => a.system_id == toSystem)) throw new ApplicationException("Destination system invalid!");

            if (fromSystem == toSystem) return null;

            var distanceMap = GetDistanceMap(fromSystem);

            return GetShortestPath(new List<int>() { toSystem }, distanceMap, toSystem);
        }

        public List<int> GetSystemsInRange(int fromSystem, int range)
        {
            if (range <= 0 || range >= 250) throw new ApplicationException("Dude... wtf are you even trying to do?");

            if (!_ctx.systems.Any(a => a.system_id == fromSystem)) throw new ApplicationException("Initial system invalid!");

            return GetDistanceMap(fromSystem).Where(d => d.Value <= range).Select(d => d.Key).ToList();
        }

        private List<int> GetShortestPath(List<int> path, Dictionary<int, int> distanceMap, int toSystem)
        {
            int currentDistance = distanceMap[toSystem];
            int currentSys = toSystem;
            while (currentDistance > 0)
            {
                if (!_systemDestinations.ContainsKey(currentSys))
                {
                    return null;
                }

                Dictionary<int, int> distances = new Dictionary<int, int>();
                int lowestDistance = currentDistance;
                foreach (int destinationSystemID in _systemDestinations[currentSys])
                {
                    // Does the system exist in the map?
                    if (distanceMap.ContainsKey(destinationSystemID))
                    {
                        int dist = distanceMap[destinationSystemID];
                        distances[destinationSystemID] = dist;
                        lowestDistance = Math.Min(lowestDistance, dist);
                    }
                }

                currentDistance = lowestDistance;
                foreach (var dest in distances.Where(g => g.Value > lowestDistance))
                    distances.Remove(dest.Key);

                if (distances.Count == 0)
                {
                    return null;
                }
                else if (distances.Count == 1)
                {
                    int dest = distances.ElementAt(0).Key;
                    if (dest != currentSys)
                    {
                        currentSys = dest;
                        if (path.Contains(currentSys)) return null;
                        path.Add(currentSys);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    int tempCurrentSys;
                    List<int> succPath = null;
                    foreach (int dest in distances.Keys)
                    {
                        List<int> tempPath = path;

                        tempCurrentSys = dest;
                        if (path.Contains(tempCurrentSys)) continue;

                        tempPath.Add(tempCurrentSys);

                        path = GetShortestPath(tempPath, distanceMap, tempCurrentSys);

                        if (succPath == null)
                        {
                            succPath = tempPath;
                        }
                        else
                        {
                            if (tempPath != null)
                            {
                                if (tempPath.Count < succPath.Count)
                                {
                                    succPath = tempPath;
                                }
                            }
                        }
                    }
                    return succPath;
                }
            }
            return path;
        }

        private Dictionary<int, int> GetDistanceMap(int fromSystem)
        {
            var data = (from s in _ctx.systems
                        join sp in _ctx.stargates on s.system_id equals sp.system_id
                        join st in _ctx.stargates on sp.destination_stargate_id equals st.stargate_id
                        orderby s.system_id
                        select new
                        {
                            fromSystem = s.system_id,
                            toSystem = st.system_id
                        }).ToList();

            if (_systemDestinations == null)
            {
                _systemDestinations = new Dictionary<int, List<int>>();
                foreach (var datum in data)
                {
                    if (!_systemDestinations.ContainsKey(datum.fromSystem)) _systemDestinations[datum.fromSystem] = new List<int>();
                    _systemDestinations[datum.fromSystem].Add(datum.toSystem);
                }
            }


            Dictionary<int, int> distanceMap = new Dictionary<int, int>();
            List<int> systemQueue = new List<int>();

            // Add this system as the seed location.
            distanceMap[fromSystem] = 0;
            systemQueue.Add(fromSystem);

            // Start interating the systems.
            while (systemQueue.Count > 0)
            {
                // Get the first unproccessed system.
                int _solarSystemID = systemQueue[0];
                systemQueue.Remove(_solarSystemID);

                // Get the next distance.
                int distance = distanceMap[_solarSystemID] + 1;

                // Iterate the gates in this system.
                List<int> gateSystems = new List<int>();
                if (!_systemDestinations.ContainsKey(_solarSystemID))
                {
                    // No jumps in this system.
                    continue;
                }

                foreach (int destinationSystemID in _systemDestinations[_solarSystemID])
                {

                    // We don't care about security.
                    gateSystems.Add(destinationSystemID);
                }
                // Process the destination systems.
                foreach (int destinationSystemID in gateSystems)
                {
                    // Does the system exist in the map?
                    if (distanceMap.ContainsKey(destinationSystemID))
                    {
                        // Was this a shorter route?
                        if (distanceMap[destinationSystemID] > distance)
                        {
                            // Yes, save the new distance.
                            distanceMap[destinationSystemID] = distance;
                            // Are we still waiting to map this system?
                            if (!systemQueue.Contains(destinationSystemID))
                            {
                                // No, so we need to redo this system.
                                systemQueue.Add(destinationSystemID);
                            }
                        }
                    }
                    else
                    {
                        // We have not reached this system before.
                        distanceMap[destinationSystemID] = distance;
                        // Add the system to parse list.
                        systemQueue.Add(destinationSystemID);
                    }
                }
            }

            return distanceMap;
        }
    }
}
