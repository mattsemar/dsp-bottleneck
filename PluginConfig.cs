using BepInEx.Configuration;

namespace Bottleneck
{
    public static class PluginConfig
    {
        public static ConfigEntry<bool> enable;

        public static void InitConfig(ConfigFile confFile)
        {
            enable = confFile.Bind("General", "enablePlugin", true,
                "Allows runtime disabling of plugin (with config manager)");
        }
    }
}