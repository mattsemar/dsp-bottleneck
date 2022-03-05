using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using static Bottleneck.Util.Log;

namespace Bottleneck.Logistics
{
    public class StationProductInfo
    {
        public int ItemCount;
        public int ItemId;
        public int ProliferatorPoints;
    }

    public enum StationType
    {
        PLS,
        ILS
    }

    public class PlanetInfo
    {
        public VectorLF3 lastLocation;
        public string Name;
        public int PlanetId;
    }

    public class StationInfo
    {
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<int, StationInfo>> pool = new();
        private readonly StationProductInfo[] _products = new StationProductInfo[15];

        public Vector3 LocalPosition;
        public PlanetInfo PlanetInfo;
        public string PlanetName;
        public int StationId;
        public bool IsOrbitalCollector;
        public StationType StationType;

        private readonly StationProductInfo[] _localExports = new StationProductInfo[15];
        private readonly StationProductInfo[] _remoteExports = new StationProductInfo[15];

        private readonly StationProductInfo[] _requestedItems = new StationProductInfo[15];
        private readonly StationProductInfo[] _suppliedItems = new StationProductInfo[15];
        private readonly ConcurrentDictionary<int, int> _itemToIndex = new();
        private readonly ConcurrentDictionary<int, int> _indexToItem = new();

        private StationInfo(Guid guid)
        {
            this.guid = guid;
        }

        public List<StationProductInfo> Products
        {
            get
            {
                var result = new List<StationProductInfo>();
                for (int i = 0; i < _products.Length; i++)
                {
                    var stationProductInfo = _products[i];
                    if (stationProductInfo == null)
                        continue;
                    result.Add(stationProductInfo);
                }

                return result;
            }
        }

        public Guid guid { get; }

        public static StationInfo Build(StationComponent station, PlanetData planet)
        {
            if (!pool.TryGetValue(planet.id, out var planetPool) || planetPool == null)
            {
                planetPool = new ConcurrentDictionary<int, StationInfo>();
                pool[planet.id] = planetPool;
            }

            if (!planetPool.TryGetValue(station.id, out var stationInfo))
            {
                stationInfo = new StationInfo(Guid.NewGuid())
                {
                    PlanetName = planet.displayName,
                    StationType = station.isStellar ? StationType.ILS : StationType.PLS,
                    StationId = station.id,
                    IsOrbitalCollector = station.isCollector
                };
                planetPool[station.id] = stationInfo;
            }

            stationInfo.PlanetInfo = new PlanetInfo
            {
                lastLocation = planet.uPosition,
                Name = planet.displayName,
                PlanetId = planet.id
            };
            stationInfo.LocalPosition = station.shipDockPos;

            for (int i = 0; i < station.storage.Length; i++)
            {
                var store = station.storage[i];
                if (store.itemId < 1)
                {
                    if (stationInfo._indexToItem.ContainsKey(i))
                    {
                        var oldItemId = stationInfo._indexToItem[i];
                        stationInfo._indexToItem.TryRemove(i, out _);
                        stationInfo._itemToIndex.TryRemove(oldItemId, out _);
                    }

                    continue;
                }

                stationInfo._indexToItem[i] = store.itemId;
                stationInfo._itemToIndex[store.itemId] = i;

                var productInfo = new StationProductInfo
                {
                    ItemId = store.itemId,
                    ItemCount = store.count,
                    ProliferatorPoints = store.inc
                };
                stationInfo._products[i] = productInfo;

                if (store.totalOrdered < 0)
                {
                    // these are already spoken for so take them from total
                    productInfo.ItemCount = Math.Max(0, productInfo.ItemCount + store.totalOrdered);
                }

                var isSupply = false;
                bool isDemand = store.remoteLogic == ELogisticStorage.Demand;

                if (store.remoteLogic == ELogisticStorage.Supply)
                {
                    isSupply = true;
                    stationInfo._remoteExports[i] = productInfo;
                }
                else
                {
                    stationInfo._remoteExports[i] = null;
                }

                if (store.localLogic == ELogisticStorage.Supply)
                {
                    isSupply = true;
                    stationInfo._localExports[i] = productInfo;
                }
                else
                {
                    stationInfo._localExports[i] = null;
                }

                if (store.localLogic == ELogisticStorage.Demand)
                {
                    isDemand = true;
                }

                stationInfo._suppliedItems[i] = null;
                stationInfo._requestedItems[i] = null;
                if (isSupply)
                {
                    if (productInfo.ItemCount > 0)
                    {
                        stationInfo._suppliedItems[i] = productInfo;
                    }
                }

                if (isDemand)
                {
                    stationInfo._requestedItems[i] = productInfo;
                }
            }


            return stationInfo;
        }


        public bool HasItem(int itemId) => _itemToIndex.ContainsKey(itemId);

        public bool HasAnyExport(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return (_localExports[index] != null && _localExports[index].ItemCount > 0)
                       || (_remoteExports[index] != null && _remoteExports[index].ItemCount > 0);
            }

            return false;
        }

        public bool HasLocalExport(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return _localExports[index] != null && _localExports[index].ItemCount > 0;
            }

            return false;
        }

        public bool HasRemoteExport(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return _remoteExports[index] != null && _remoteExports[index].ItemCount > 0;
            }

            return false;
        }

        public StationProductInfo GetProductInfo(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return _products[index];
            }

            return null;
        }

        public bool IsSupplied(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return _suppliedItems[index] != null && _suppliedItems[index].ItemCount > 0;
            }

            return false;
        }

        public bool IsRequested(int itemId)
        {
            if (_itemToIndex.TryGetValue(itemId, out int index))
            {
                return _requestedItems[index] != null;
            }

            return false;
        }
    }

    public class ByItemSummary
    {
        public int AvailableItems;
        public int Requesters;
        public int SuppliedItems;
        public int Suppliers;

        public static ByItemSummary operator +(ByItemSummary summary1, ByItemSummary summary2)
        {
            return new ByItemSummary
            {
                AvailableItems = summary1.AvailableItems + summary2.AvailableItems,
                Requesters = summary1.Requesters + summary2.Requesters,
                Suppliers = summary1.Suppliers + summary2.Suppliers,
                SuppliedItems = summary1.SuppliedItems + summary2.SuppliedItems
            };
        }
    }

    public static class LogisticsNetwork
    {
        private static readonly List<StationInfo> _stations = new();
        private static ConcurrentDictionary<Guid, StationInfo> _knownStations = new();
        private static readonly ConcurrentDictionary<int, ByItemSummary> byItemSummary = new();
        public static bool IsInitted;
        public static bool IsRunning;
        public static bool IsFirstLoadComplete;
        private static Timer _timer;
        private static readonly Dictionary<int, Dictionary<int, ByItemSummary>> byPlanetByItem = new();

        public static void Reset()
        {
            lock (_stations)
            {
                _stations.Clear();
            }

            _knownStations.Clear();
            IsFirstLoadComplete = false;
            IsInitted = false;
            byItemSummary.Clear();
        }

        public static ByItemSummary ForItemId(int itemId, int astroId)
        {
            if (astroId == -1)
            {
                byItemSummary.TryGetValue(itemId, out var summary);
                return summary;
            }
            else if (astroId == 0 || astroId % 100 > 0)
            {
                var planet = astroId == 0 ? GameMain.localPlanet.id : astroId;
                byPlanetByItem.TryGetValue(planet, out var planetSummary);
                if (planetSummary != null)
                {
                    planetSummary.TryGetValue(itemId, out var summary);
                    return summary;
                }

                return null;
            }
            else if (astroId % 100 == 0)
            {
                int starId = astroId / 100;
                StarData starData = GameMain.data.galaxy.StarById(starId);
                ByItemSummary result = new ByItemSummary();
                for (int j = 0; j < starData.planetCount; j++)
                {
                    if (starData.planets[j].factory != null)
                    {
                        var planet = starData.planets[j].id;
                        byPlanetByItem.TryGetValue(planet, out var planetSummary);
                        if (planetSummary != null)
                        {
                            planetSummary.TryGetValue(itemId, out var summary);
                            if (summary != null)
                            {
                                result += summary;
                            }
                        }
                    }
                }
            }

            return null;
        }


        public static List<StationInfo> stations
        {
            get
            {
                lock (_stations)
                {
                    return _stations;
                }
            }
        }

        public static void Start()
        {
            _timer = new Timer(6_000);
            _timer.Elapsed += DoPeriodicTask;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            IsInitted = true;
        }

        private static void DoPeriodicTask(object source, ElapsedEventArgs e)
        {
            try
            {
                if (PluginConfig.IsPaused())
                {
                    return;
                }

                CollectStationInfos(source, e);
            }
            catch (Exception exc)
            {
                Warn($"exception in periodic task {exc.Message}\n{exc.StackTrace}");
            }
        }

        private static void CollectStationInfos(object source, ElapsedEventArgs e)
        {
            if (IsRunning)
            {
                logger.LogWarning("Collect already running");
                return;
            }

            IsRunning = true;
            var newByItemSummary = new Dictionary<int, ByItemSummary>();
            try
            {
                foreach (var star in GameMain.universeSimulator.galaxyData.stars)
                {
                    foreach (var planet in star.planets)
                    {
                        if (planet.factory != null && planet.factory.factorySystem != null &&
                            planet.factory.transport != null &&
                            planet.factory.transport.stationCursor != 0)
                        {
                            var newPlanetSummary = new Dictionary<int, ByItemSummary>();
                            var transport = planet.factory.transport;
                            for (var i = 1; i < transport.stationCursor; i++)
                            {
                                var station = transport.stationPool[i];
                                if (station == null || station.id != i)
                                {
                                    continue;
                                }

                                var stationInfo = StationInfo.Build(station, planet);
                                if (!_knownStations.ContainsKey(stationInfo.guid))
                                {
                                    if (IsFirstLoadComplete)
                                    {
                                        Warn($"Creating a new station after first load complete. Maybe station is actually new {planet.displayName}");
                                    }

                                    lock (_stations)
                                    {
                                        _stations.Add(stationInfo);
                                    }

                                    _knownStations.TryAdd(stationInfo.guid, stationInfo);
                                }

                                foreach (var productInfo in stationInfo.Products)
                                {
                                    if (productInfo == null)
                                        continue;

                                    if (newByItemSummary.TryGetValue(productInfo.ItemId, out var summary))
                                    {
                                        summary.AvailableItems += productInfo.ItemCount;

                                        if (stationInfo.IsSupplied(productInfo.ItemId))
                                        {
                                            summary.Suppliers++;
                                            summary.SuppliedItems += productInfo.ItemCount;
                                        }
                                        else
                                        {
                                            summary.Requesters++;
                                        }
                                    }
                                    else
                                    {
                                        newByItemSummary[productInfo.ItemId] = new ByItemSummary
                                        {
                                            AvailableItems = productInfo.ItemCount,
                                            Requesters = stationInfo.IsRequested(productInfo.ItemId) ? 1 : 0,
                                            Suppliers = stationInfo.IsSupplied(productInfo.ItemId) ? 1 : 0,
                                            SuppliedItems = stationInfo.IsSupplied(productInfo.ItemId) ? productInfo.ItemCount : 0,
                                        };
                                    }

                                    if (newPlanetSummary.TryGetValue(productInfo.ItemId, out var planetSummaryForItem))
                                    {
                                        planetSummaryForItem.AvailableItems += productInfo.ItemCount;

                                        if (stationInfo.IsSupplied(productInfo.ItemId))
                                        {
                                            planetSummaryForItem.Suppliers++;
                                            planetSummaryForItem.SuppliedItems += productInfo.ItemCount;
                                        }
                                        else
                                        {
                                            planetSummaryForItem.Requesters++;
                                        }
                                    }
                                    else
                                    {
                                        newPlanetSummary[productInfo.ItemId] = new ByItemSummary
                                        {
                                            AvailableItems = productInfo.ItemCount,
                                            Requesters = stationInfo.IsRequested(productInfo.ItemId) ? 1 : 0,
                                            Suppliers = stationInfo.IsSupplied(productInfo.ItemId) ? 1 : 0,
                                            SuppliedItems = stationInfo.IsSupplied(productInfo.ItemId) ? productInfo.ItemCount : 0,
                                        };
                                    }
                                }
                            }
                            byPlanetByItem[planet.id] = newPlanetSummary;
                        }
                    }
                }

                IsInitted = true;
                IsFirstLoadComplete = true;
            }
            catch (Exception err)
            {
                logger.LogWarning($"Collection task failed {err} {err.StackTrace}");
            }
            finally
            {
                byItemSummary.Clear();
                foreach (var itemId in newByItemSummary.Keys)
                {
                    byItemSummary.TryAdd(itemId, newByItemSummary[itemId]);
                }

                IsRunning = false;
            }
        }


        public static void Stop()
        {
            IsInitted = false;
            IsFirstLoadComplete = false;
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            _timer.Dispose();
        }

        public static bool HasItem(int itemId) => byItemSummary.ContainsKey(itemId);
    }
}