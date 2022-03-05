using System;
using System.Linq;
using Bottleneck.Util;

namespace Bottleneck.Stats
{
    public static class ResearchTechHelper
    {
        private static TechProto _sprayLevel3Proto;
        private static TechProto _sprayLevel2Proto;
        private static TechProto _sprayLevel1Proto;
        private const int PlanetLogisticStationId = 2103;
        private const int StellarLogisticStationId = 2104;
        private const int Warper = 1210;
        private static DateTime _lastCheckedUnlockedState = DateTime.Now.AddDays(-1);
        private static bool _cachedLogisticsUnlockedValue;
        private static bool _cachedStellarUnlockedValue;

        public static float GetMaxProductivityIncrease()
        {
            var highestProliferatorTechUnlocked = GetMaxIncIndex();
            return (float)Cargo.incTableMilli[highestProliferatorTechUnlocked];
        }

        public static float GetMaxSpeedIncrease()
        {
            var highestProliferatorTechUnlocked = GetMaxIncIndex();
            return (float)Cargo.accTableMilli[highestProliferatorTechUnlocked];
        }

        public static int GetMaxIncIndex()
        {
            InitTechProtos();
            if (GameMain.history.techStates[_sprayLevel3Proto.ID].unlocked)
                return 4;
            if (GameMain.history.techStates[_sprayLevel2Proto.ID].unlocked)
                return 2;
            if (GameMain.history.techStates[_sprayLevel1Proto.ID].unlocked)
                return 1;
            return 0;
        }

        private static void InitTechProtos()
        {
            if (_sprayLevel3Proto == null)
            {
                var proliferatorProtos = LDB.techs.dataArray.ToList().FindAll(t => t.Name.Contains("增产剂"));
                proliferatorProtos.Sort((p1, p2) =>
                {
                    if (p1.PreTechs.Contains(p2.ID))
                    {
                        // sorting high to low
                        return -1;
                    }

                    if (p2.PreTechs.Contains(p1.ID))
                    {
                        return 1;
                    }

                    return p1.ID.CompareTo(p2.ID);
                });
                if (proliferatorProtos.Count >= 3)
                {
                    // if more are added, add them here
                    _sprayLevel3Proto = proliferatorProtos[0];
                    _sprayLevel2Proto = proliferatorProtos[1];
                    _sprayLevel1Proto = proliferatorProtos[2];
                }
            }
        }

        public static bool IsProliferatorUnlocked()
        {
            return GetMaxIncIndex() > 0;
        }

        public static int GetMaxPilerStackingUnlocked()
        {
            if (PluginConfig.disableStackingCalc.Value)
            {
                return 1;
            }
            var stationPilerLevel1 = GameMain.history.TechUnlocked(3801) ? 1 + (int)LDB.techs.Select(3801).UnlockValues[0] : 1;
            var stationPilerLevel2 = GameMain.history.TechUnlocked(3802) ? stationPilerLevel1 + (int)LDB.techs.Select(3802).UnlockValues[0] : stationPilerLevel1;
            var maxStationPilerTech = GameMain.history.TechUnlocked(3803) ? stationPilerLevel2 + (int)LDB.techs.Select(3803).UnlockValues[0] : stationPilerLevel2;
            return maxStationPilerTech;
        }

        public static bool IsLogisticsUnlocked()
        {
            UpdateCachedUnlockValues();
            return _cachedLogisticsUnlockedValue;
        }

        public static bool IsStellarLogisticsUnlocked()
        {
            UpdateCachedUnlockValues();
            return _cachedStellarUnlockedValue;
        }
        
        private static void UpdateCachedUnlockValues()
        {
            if (!_cachedLogisticsUnlockedValue && (DateTime.Now - _lastCheckedUnlockedState).TotalMinutes > 10)
            {
                _cachedLogisticsUnlockedValue = GameMain.history.ItemUnlocked(PlanetLogisticStationId);
                _cachedStellarUnlockedValue = GameMain.history.ItemUnlocked(StellarLogisticStationId) && GameMain.history.ItemUnlocked(Warper);
                Log.Debug($"cached logistics unlocked {_cachedLogisticsUnlockedValue}");
                _lastCheckedUnlockedState = DateTime.Now;
            }
        }
    }
}
