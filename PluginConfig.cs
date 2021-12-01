using BepInEx.Configuration;

namespace Bottleneck
{
    public static class PluginConfig
    {
        public static ConfigEntry<int> productionPlanetCount;

        public static void InitConfig(ConfigFile confFile)
        {
            productionPlanetCount = confFile.Bind("General", "ProductionPlanetCount", 5, new ConfigDescription(
                "Number of production planets to show. Too many and tip gets very large",
                new AcceptableValueRange<int>(2, 15)));
        }
    }
}