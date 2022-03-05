using BepInEx.Configuration;

namespace Bottleneck
{
    public static class PluginConfig
    {
        public static ConfigEntry<int> productionPlanetCount;
        public static ConfigEntry<bool> popupLowPowerWarnings;
        public static ConfigEntry<float> lackOfProductionRatioTrigger;
        public static ConfigEntry<float> consumptionToProductionRatioTrigger;
        public static ConfigEntry<bool> displayPerSecond;
        public static ConfigEntry<bool> disableProliferatorCalc;
        public static ConfigEntry<bool> disableStackingCalc;
        public static ConfigEntry<bool> statsOnly;


        public static void InitConfig(ConfigFile confFile)
        {
            productionPlanetCount = confFile.Bind("General", "ProductionPlanetCount", 5, new ConfigDescription(
                "Number of production planets to show. Too many and tip gets very large",
                new AcceptableValueRange<int>(2, 35)));
            popupLowPowerWarnings = confFile.Bind("General", "PopupLowPowerWarnings", true, "When planets with too little power are detected a message will be popped up (once per session)");
            
            lackOfProductionRatioTrigger = confFile.Bind("General", "lackOfProductionRatio", 0.9f, //
                "When consumption rises above the given ratio of max production, flag the text in red." +//
                " (e.g. if set to '0.9' then you will be warned if you consume more than 90% of your max production)");
            consumptionToProductionRatioTrigger = confFile.Bind("General", "consumptionToProductionRatio", 1.5f, //
                "If max consumption raises above the given max production ratio, flag the text in red." +//
                " (e.g. if set to '1.5' then you will be warned if your max consumption is more than 150% of your max production)");
            displayPerSecond = confFile.Bind("General", "displayPerSecond", false,
                "Used by UI to persist the last selected value for checkbox");
            disableProliferatorCalc = confFile.Bind("General", "Disable Proliferator Calculation", false,
                "Tells mod to ignore proliferator points completely. Can cause production rates to exceed theoretical max values");
            disableStackingCalc = confFile.Bind("General", "Disable Stacking Calculation", false,
                "Tells mod to ignore unlocked tech for stacking items on belts. By default uses same 'Tech Limit' value as stations use");
            statsOnly = confFile.Bind("Stats", "Disable Bottleneck", false,
                "Disable Bottleneck functionality, use only BetterStats features");
        }

        private static bool _paused;

        public static void SetRunningState(bool running)
        {
            _paused = !running;
        }
        
        public static bool IsPaused()
        {
            return _paused;
        }
    }
}