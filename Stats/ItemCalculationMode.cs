using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using Bottleneck.Util;
using UnityEngine;

namespace Bottleneck.Stats
{
    public enum ItemCalculationMode
    {
        None,
        Normal,
        ForceSpeed,
        ForceProductivity
    }

    /// <summary>
    /// Manages currently selected proliferator calculation options for each item
    /// </summary>
    public class ItemCalculationRuntimeSetting
    {
        public static readonly ItemCalculationRuntimeSetting None = new(0)
        {
            _enabled = false,
            _mode = ItemCalculationMode.None,
        };

        private ItemCalculationMode _mode = ItemCalculationMode.Normal;
        private bool _enabled;

        public readonly int productId;

        private ConfigEntry<string> _configEntry;
        private static readonly Dictionary<int, ConfigEntry<string>> ConfigEntries = new();
        private static readonly Dictionary<int, ItemCalculationRuntimeSetting> Pool = new();
        private static ConfigFile configFile;
        private readonly ItemProto _itemProto;

        private ItemCalculationRuntimeSetting(int productId)
        {
            this.productId = productId;
            var proto = LDB.items.Select(productId);
            if (proto != null)
            {
                _itemProto = proto;
            }
        }

        public ItemCalculationMode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                Pool[productId]._mode = value;
                Save();
            }
        }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enabled = value;
                Pool[productId]._enabled = value;
                Save();
            }
        }

        public bool SpeedSupported => _itemProto is { recipes: { Count: > 0 } };

        public bool ProductivitySupported
        {
            get { return SpeedSupported && _itemProto.recipes.Any(r => r.productive); }
        }

        private void Save()
        {
            _configEntry.Value = Serialize();
            Log.Debug($"saved {productId} entry {_configEntry.Value}");
        }

        private static ItemCalculationRuntimeSetting Deserialize(string strVal)
        {
            var serializableRuntimeState = JsonUtility.FromJson<SerializableRuntimeState>(strVal);

            return new ItemCalculationRuntimeSetting(serializableRuntimeState.productId)
            {
                _enabled = serializableRuntimeState.enabled,
                _mode = (ItemCalculationMode)serializableRuntimeState.mode,
            };
        }

        private string Serialize()
        {
            return JsonUtility.ToJson(SerializableRuntimeState.From(this));
        }

        public static void InitConfig()
        {
            configFile = new ConfigFile($"{Paths.ConfigPath}/{PluginInfo.PLUGIN_NAME}/CustomProductSettings.cfg", true);

            foreach (var itemProto in LDB.items.dataArray)
            {
                var defaultValue = new ItemCalculationRuntimeSetting(itemProto.ID)
                {
                    _enabled = true,
                    _mode = ItemCalculationMode.Normal
                };

                var configEntry = configFile.Bind("Internal", $"ProliferatorStatsSetting_{itemProto.ID}",
                    defaultValue.Serialize(),
                    "For internal use only");
                ConfigEntries[itemProto.ID] = configEntry;

                Pool[itemProto.ID] = Deserialize(ConfigEntries[itemProto.ID].Value);
                Pool[itemProto.ID]._configEntry = configEntry;
            }
        }

        public static ItemCalculationRuntimeSetting ForItemId(int itemId)
        {
            if (PluginConfig.disableProliferatorCalc.Value)
                return None;
            if (Pool.ContainsKey(itemId))
                return Pool[itemId];
            
            Log.Info($"Found item id not previously created {itemId}");
            var defaultValue = new ItemCalculationRuntimeSetting(itemId)
            {
                _enabled = true,
                _mode = ItemCalculationMode.Normal
            };
            
            var configEntry = configFile.Bind("Internal", $"ProliferatorStatsSetting_{itemId}",
                defaultValue.Serialize(),
                "For internal use only");
            ConfigEntries[itemId] = configEntry;

            Pool[itemId] = Deserialize(ConfigEntries[itemId].Value);
            Pool[itemId]._configEntry = configEntry;
            return Pool[itemId];
        }

        public static void OutputModes(out int[] productIds, out short[] modes)
        {
            int i = 0, num = Pool.Count;
            productIds = new int[num];
            modes = new short[num];
            foreach (var pair in Pool)
            {
                productIds[i] = pair.Key;
                modes[i] = (short)((pair.Value._enabled ? 1 : 0) + ((int)pair.Value._mode << 1));
                i++;
            }
        }

        public static void InputModes(in int[] productIds, in short[] modes)
        {
            int num = Pool.Count;
            for (int i = 0; i < productIds.Length; i++)
            {
                int id = productIds[i];
                Pool[id]._enabled = (modes[i] & 1) > 0;
                Pool[id]._mode = ((ItemCalculationMode)(modes[i] >> 1));
            }
        }
    }

    [Serializable]
    public class SerializableRuntimeState
    {
        [SerializeField] public int mode;
        [SerializeField] public bool enabled;
        [SerializeField] public int productId;

        public SerializableRuntimeState(int productId, bool enabled, ItemCalculationMode mode)
        {
            this.productId = productId;
            this.enabled = enabled;
            this.mode = (int)mode;
        }

        public static SerializableRuntimeState From(ItemCalculationRuntimeSetting setting)
        {
            return new SerializableRuntimeState(setting.productId, setting.Enabled, setting.Mode);
        }
    }
}