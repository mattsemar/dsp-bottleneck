using System;
using System.Collections.Generic;
using BepInEx;
using Bottleneck.UI;
using Bottleneck.Util;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Bottleneck
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.brokenmass.plugin.DSP.BetterStats")]
    public class BottleneckPlugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        private static BottleneckPlugin _instance;
        private GameObject _enablePrecursorGO;
        private Image _precursorCheckBoxImage;
        private static readonly Texture2D filterTexture = Resources.Load<Texture2D>("ui/textures/sprites/icons/filter-icon-16");
        private GameObject _enablePrecursorTextGO;
        private readonly HashSet<int> _itemFilter = new();
        private readonly Dictionary<int, PlanetaryProductionSummary> _productionLocations = new();
        private readonly HashSet<ProductionKey> _countedProducers = new();
        private readonly HashSet<ProductionKey> _countedConsumers = new();
        private List<GameObject> objsToDestroy = new();

        private readonly Dictionary<UIProductEntry, BottleneckProductEntryElement> _uiElements = new();
        private int _targetItemId = -1;
        private bool _deficientOnlyMode;
        private GameObject _textGo;
        private Button _btn;
        private Sprite _filterSprite;
        private bool _enableMadeOn;
        private bool _madeOnComputedSinceOpen;
        private bool _deficitComputedSinceOpen;
        private BottleneckTask _pendingMadeOnTask;
        private BottleneckTask _pendingDeficitTask;
        private Dictionary<UIButton, FilterButtonItemAge> _buttonTipAge = new();

        private void Awake()
        {
            Log.logger = Logger;
            _instance = this;
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            _harmony.PatchAll(typeof(BottleneckPlugin));
            PluginConfig.InitConfig(Config);
            Log.Info($"Plugin {PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        private void Update()
        {
            if (_pendingMadeOnTask != null)
            {
                var task = _pendingMadeOnTask;
                _pendingMadeOnTask = null;
                if (!_madeOnComputedSinceOpen)
                {
                    Log.Debug($"Processing madeOn task, age: {DateTime.Now - task.createdAt}");
                    ProcessMadeOnTask(task);
                    _madeOnComputedSinceOpen = true;
                }
                else
                {
                    Log.Debug("Skipping madeOn task since window has not been opened since last attempt");
                }
            }

            if (_pendingDeficitTask != null)
            {
                var task = _pendingDeficitTask;
                _pendingDeficitTask = null;
                Log.Debug($"Processing deficit task, age: {DateTime.Now - task.createdAt}");
                ProcessDeficitTask(task);
                _deficitComputedSinceOpen = true;
            }
        }

        private void ProcessMadeOnTask(BottleneckTask task)
        {
            var uiStatsWindow = task.statsWindow;
            if (uiStatsWindow == null || uiStatsWindow.gameObject == null || !uiStatsWindow.gameObject.activeSelf)
            {
                Log.Debug($"skipping madeon task due to window not being active");
                return;
            }

            _productionLocations.Clear();
            _countedConsumers.Clear();
            _countedProducers.Clear();
            for (int i = 0; i < uiStatsWindow.gameData.factoryCount; i++)
            {
                AddPlanetFactoryData(uiStatsWindow.gameData.factories[i], true);
            }

            _enableMadeOn = true;
        }

        private void ProcessDeficitTask(BottleneckTask task)
        {
            var uiStatsWindow = task.statsWindow;
            if (uiStatsWindow == null || uiStatsWindow.gameObject == null || !uiStatsWindow.gameObject.activeSelf)
            {
                Log.Debug($"skipping deficit task due to window not being active");
                return;
            }

            ProductionDeficit.Clear();

            if (uiStatsWindow.astroFilter == -1)
            {
                int factoryCount = uiStatsWindow.gameData.factoryCount;
                for (int i = 0; i < factoryCount; i++)
                {
                    AddPlanetFactoryData(uiStatsWindow.gameData.factories[i], false);
                }
            }
            else if (uiStatsWindow.astroFilter == 0)
            {
                AddPlanetFactoryData(uiStatsWindow.gameData.localPlanet.factory, false);
            }
            else if (uiStatsWindow.astroFilter % 100 > 0)
            {
                PlanetData planetData = uiStatsWindow.gameData.galaxy.PlanetById(uiStatsWindow.astroFilter);
                AddPlanetFactoryData(planetData.factory, false);
            }
            else if (uiStatsWindow.astroFilter % 100 == 0)
            {
                int starId = uiStatsWindow.astroFilter / 100;
                StarData starData = uiStatsWindow.gameData.galaxy.StarById(starId);
                for (int j = 0; j < starData.planetCount; j++)
                {
                    if (starData.planets[j].factory != null)
                    {
                        AddPlanetFactoryData(starData.planets[j].factory, false);
                    }
                }
            }
        }

        internal void OnDestroy()
        {
            // For ScriptEngine hot-reloading
            _itemFilter.Clear();
            _targetItemId = -1;
            _productionLocations.Clear();

            if (_enablePrecursorGO != null)
            {
                Destroy(_enablePrecursorTextGO);
                Destroy(_enablePrecursorGO);
            }

            if (_textGo != null)
            {
                Destroy(_textGo);
            }

            if (_precursorCheckBoxImage != null)
            {
                Destroy(_precursorCheckBoxImage.gameObject);
            }

            if (_btn != null && _btn.gameObject != null)
                Destroy(_btn.gameObject);

            Clear();
            _harmony.UnpatchSelf();
        }

        private void Clear()
        {
            foreach (var obj in objsToDestroy)
            {
                Destroy(obj);
            }

            objsToDestroy.Clear();

            foreach (BottleneckProductEntryElement element in _uiElements.Values)
            {
                if (element.precursorButton != null)
                    Destroy(element.precursorButton.gameObject);
                if (element.successorButton != null)
                    Destroy(element.successorButton.gameObject);
            }

            _uiElements.Clear();
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIStatisticsWindow), "_OnOpen")]
        public static void UIStatisticsWindow__OnOpen_Postfix(UIStatisticsWindow __instance)
        {
            if (_instance != null && _instance != null && _instance.gameObject != null)
            {
                _instance.AddEnablePrecursorFilterButton(__instance);
                _instance._madeOnComputedSinceOpen = false;
                _instance._deficitComputedSinceOpen = false;
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIProductEntryList), "FilterEntries")]
        public static void UIProductEntryList_FilterEntries_Postfix(UIProductEntryList __instance)
        {
            if (_instance != null)
            {
                _instance.FilterEntries(__instance);
            }
        }


        [HarmonyPrefix, HarmonyPatch(typeof(UIStatisticsWindow), "_OnUpdate")]
        public static void UIStatisticsWindow__OnUpdate_Prefix(UIStatisticsWindow __instance)
        {
            if (_instance == null)
                return;
            _instance.UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (_targetItemId == -1)
            {
                _btn.gameObject.SetActive(false);
                _textGo.SetActive(false);
            }
            else
            {
                _btn.gameObject.SetActive(true);
                _textGo.SetActive(true);
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIProductEntry), "_OnUpdate")]
        public static void UIProductEntry__OnUpdate_Postfix(UIProductEntry __instance)
        {
            if (_instance != null)
            {
                _instance.OnUpdate(__instance);
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(UIStatisticsWindow), "ComputeDisplayEntries")]
        public static void UIProductionStatWindow_ComputeDisplayEntries_Prefix(UIStatisticsWindow __instance)
        {
            if (_instance == null || __instance == null)
                return;
            _instance.RecordEntryData(__instance);
        }

        private void RecordEntryData(UIStatisticsWindow uiStatsWindow)
        {
            bool planetUsageMode = Time.frameCount % 500 == 0;
            bool deficitMode = Time.frameCount % 102 == 0;
            if ((!planetUsageMode && !deficitMode) && (_deficitComputedSinceOpen && _madeOnComputedSinceOpen))
            {
                // no need to run every frame
                return;
            }

            if (planetUsageMode || !_enableMadeOn)
            {
                _pendingMadeOnTask = new BottleneckTask
                {
                    statsWindow = uiStatsWindow,
                    taskType = TaskType.MadeOn,
                };
            }

            if (deficitMode && _pendingDeficitTask == null)
            {
                _pendingDeficitTask = new BottleneckTask
                {
                    statsWindow = uiStatsWindow,
                    taskType = TaskType.Deficit
                };
            }
        }

        private void OnUpdate(UIProductEntry productEntry)
        {
            if (productEntry.productionStatWindow == null || !productEntry.productionStatWindow.isProductionTab) return;

            if (!_uiElements.TryGetValue(productEntry, out BottleneckProductEntryElement elt))
            {
                elt = EnhanceElement(productEntry);
            }

            if (elt.precursorButton != null && ButtonOutOfDate(elt.precursorButton, productEntry.entryData.itemId))
            {
                var productId = productEntry.entryData.itemId;
                elt.precursorButton.tips.tipTitle = "Production Details";
                if (ItemUtil.HasPrecursors(productId))
                {
                    elt.precursorButton.tips.tipTitle += " (click to show only precursor items)";
                }

                if (_productionLocations.ContainsKey(productId))
                {
                    if (_enableMadeOn)
                    {
                        var parensMessage = ItemUtil.HasPrecursors(productId) ? "(Control click see only precursors that are lacking)\r\n" : "";
                        elt.precursorButton.tips.tipText = $"{parensMessage}<b>Produced on</b>\r\n" + _productionLocations[productId].GetProducerSummary();
                        if (_productionLocations[productId].PlanetCount() > PluginConfig.productionPlanetCount.Value)
                        {
                            elt.precursorButton.tips.tipTitle += $" (top {PluginConfig.productionPlanetCount.Value} / {_productionLocations[productId].PlanetCount()} planets)";
                        }
                    }
                    else
                    {
                        elt.precursorButton.tips.tipText = "Production planets not shown when single planet selected";
                    }

                    var deficitItemName = ProductionDeficit.MostNeeded(productId);
                    if (deficitItemName.Length > 0)
                    {
                        elt.precursorButton.tips.tipText += $"\r\n<b>Bottlenecks</b>\r\n{deficitItemName}";
                    }
                }
                else
                {
                    elt.precursorButton.tips.tipText = "";
                }

                UpdateButtonUpdateDate(elt.precursorButton, productId);
            }

            if (elt.successorButton != null && ButtonOutOfDate(elt.successorButton, productEntry.entryData.itemId))
            {
                var productId = productEntry.entryData.itemId;
                elt.successorButton.tips.tipTitle = "Consumption Details";
                if (ItemUtil.HasConsumers(productId))
                {
                    elt.successorButton.tips.tipTitle += " (click to show only consuming items)";
                }

                if (_productionLocations.ContainsKey(productId) && _enableMadeOn)
                {
                    elt.successorButton.tips.tipText = "<b>Consumed on</b>\r\n" + _productionLocations[productId].GetConsumerSummary();
                    if (_productionLocations[productId].ConsumerPlanetCount() > PluginConfig.productionPlanetCount.Value)
                    {
                        elt.successorButton.tips.tipTitle += $" (top {PluginConfig.productionPlanetCount.Value} / {_productionLocations[productId].ConsumerPlanetCount()} planets)";
                    }
                }
                else
                {
                    elt.successorButton.tips.tipText = "";
                }

                UpdateButtonUpdateDate(elt.successorButton, productId);
            }
        }

        private void UpdateButtonUpdateDate(UIButton uiButton, int productId)
        {
            _buttonTipAge[uiButton] = new FilterButtonItemAge(uiButton, productId)
            {
                lastUpdated = DateTime.Now
            };
        }

        private bool ButtonOutOfDate(UIButton uiButton, int entryDataItemId)
        {
            if (_buttonTipAge.TryGetValue(uiButton, out FilterButtonItemAge itemAge))
            {
                if (itemAge.itemId != entryDataItemId)
                    return true;
                return (DateTime.Now - itemAge.lastUpdated).TotalSeconds > 10;
            }

            _buttonTipAge[uiButton] = new FilterButtonItemAge(uiButton, entryDataItemId)
            {
                lastUpdated = DateTime.Now
            };
            return true;
        }

        private void ClearFilter()
        {
            _itemFilter.Clear();
            _targetItemId = -1;
        }

        private BottleneckProductEntryElement EnhanceElement(UIProductEntry productEntry)
        {
            var precursorButton = UI.Util.CopyButton(productEntry, productEntry.favoriteBtn1, new Vector2(120 + 47, 80), productEntry.entryData.itemId,
                _ => { UpdatePrecursorFilter(productEntry.entryData.itemId); }, _filterSprite);

            objsToDestroy.Add(precursorButton.gameObject);
            var successorButton = UI.Util.CopyButton(productEntry, productEntry.favoriteBtn1, new Vector2(120 + 47, 0), productEntry.entryData.itemId,
                _ => { UpdatePrecursorFilter(productEntry.entryData.itemId, true); }, _filterSprite);
            objsToDestroy.Add(successorButton.gameObject);
            var result = new BottleneckProductEntryElement
            {
                precursorButton = precursorButton,
                successorButton = successorButton
            };

            _uiElements.Add(productEntry, result);

            return result;
        }

        private void UpdatePrecursorFilter(int itemId, bool successor = false)
        {
            _itemFilter.Clear();
            _itemFilter.Add(itemId);
            _targetItemId = itemId;
            _deficientOnlyMode = VFInput.control;

            if (!successor)
            {
                var directPrecursorItems = ItemUtil.DirectPrecursorItems(itemId);
                foreach (var directPrecursorItem in directPrecursorItems)
                {
                    _itemFilter.Add(directPrecursorItem);
                }
            }
            else
            {
                var successorItems = ItemUtil.DirectSuccessorItems(itemId);
                foreach (var successorItem in successorItems)
                {
                    _itemFilter.Add(successorItem);
                }
            }
        }

        private void FilterEntries(UIProductEntryList uiProductEntryList)
        {
            if (_itemFilter.Count == 0) return;
            for (int pIndex = uiProductEntryList.entryDatasCursor - 1; pIndex >= 0; --pIndex)
            {
                UIProductEntryData entryData = uiProductEntryList.entryDatas[pIndex];

                var hideItem = !_itemFilter.Contains(entryData.itemId);
                if (_deficientOnlyMode && entryData.itemId != _targetItemId)
                {
                    hideItem = !ProductionDeficit.IsDeficitItemFor(entryData.itemId, _targetItemId);
                }

                // hide the filtered item by moving it to the cursor location and decrementing cursor by one
                if (hideItem)
                {
                    uiProductEntryList.Swap(pIndex, uiProductEntryList.entryDatasCursor - 1);
                    --uiProductEntryList.entryDatasCursor;
                }
            }
        }

        private void AddEnablePrecursorFilterButton(UIStatisticsWindow uiStatisticsWindow)
        {
            if (_enablePrecursorGO != null)
                return;
            _filterSprite = Sprite.Create(filterTexture, new Rect(0, 0, filterTexture.width * 0.75f, filterTexture.height * 0.75f), new Vector2(0.5f, 0.5f));
            _enablePrecursorGO = new GameObject("enablePrecursor");
            RectTransform rect = _enablePrecursorGO.AddComponent<RectTransform>();
            rect.SetParent(uiStatisticsWindow.productSortBox.transform.parent, false);

            rect.anchorMax = new Vector2(0, 1);
            rect.anchorMin = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(20, 20);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(350, -33);
            objsToDestroy.Add(rect.gameObject);
            _btn = rect.gameObject.AddComponent<Button>();
            _btn.onClick.AddListener(ClearFilter);

            _precursorCheckBoxImage = _btn.gameObject.AddComponent<Image>();
            _precursorCheckBoxImage.color = new Color(0.8f, 0.8f, 0.8f, 1);
            _precursorCheckBoxImage.sprite = _filterSprite;


            _enablePrecursorTextGO = new GameObject("enablePrecursorText");
            RectTransform rectTxt = _enablePrecursorTextGO.AddComponent<RectTransform>();

            rectTxt.SetParent(_enablePrecursorGO.transform, false);

            rectTxt.anchorMax = new Vector2(0, 0.5f);
            rectTxt.anchorMin = new Vector2(0, 0.5f);
            rectTxt.sizeDelta = new Vector2(100, 20);
            rectTxt.pivot = new Vector2(0, 0.5f);
            rectTxt.anchoredPosition = new Vector2(20, 0);
            objsToDestroy.Add(rectTxt.gameObject);
            Text text = rectTxt.gameObject.AddComponent<Text>();
            text.text = "Clear filter";
            text.fontStyle = FontStyle.Normal;
            text.fontSize = 12;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.color = new Color(0.8f, 0.8f, 0.8f, 1);
            Font fnt = Resources.Load<Font>("ui/fonts/SAIRASB");
            if (fnt != null)
                text.font = fnt;
            _textGo = text.gameObject;
        }

        public void AddPlanetFactoryData(PlanetFactory planetFactory, bool planetUsage)
        {
            var factorySystem = planetFactory.factorySystem;
            var veinPool = planetFactory.planet.factory.veinPool;
            if (planetUsage)
                for (int i = 1; i < factorySystem.minerCursor; i++)
                {
                    var miner = factorySystem.minerPool[i];
                    if (i != miner.id) continue;

                    var productId = miner.productId;
                    var veinId = (miner.veinCount != 0) ? miner.veins[miner.currentVeinIndex] : 0;

                    if (miner.type == EMinerType.Water)
                    {
                        productId = planetFactory.planet.waterItemId;
                    }
                    else if (productId == 0)
                    {
                        productId = veinPool[veinId].productId;
                    }

                    if (productId == 0) continue;
                    AddPlanetaryUsage(productId, planetFactory.planet, miner.entityId);
                }

            for (int i = 1; i < factorySystem.assemblerCursor; i++)
            {
                var assembler = factorySystem.assemblerPool[i];
                if (assembler.id != i || assembler.recipeId == 0) continue;

                if (planetUsage)
                    foreach (var productId in assembler.requires)
                    {
                        AddPlanetaryUsage(productId, planetFactory.planet, assembler.entityId, true);
                    }

                foreach (var productId in assembler.products)
                {
                    if (planetUsage) AddPlanetaryUsage(productId, planetFactory.planet, assembler.entityId);
                    else ProductionDeficit.RecordDeficit(productId, assembler, planetFactory);
                }
            }


            if (planetUsage)
                for (int i = 1; i < factorySystem.fractionateCursor; i++)
                {
                    var fractionator = factorySystem.fractionatePool[i];
                    if (fractionator.id != i) continue;

                    if (fractionator.fluidId != 0)
                    {
                        var productId = fractionator.fluidId;
                        AddPlanetaryUsage(productId, planetFactory.planet, fractionator.entityId, true);
                    }

                    if (fractionator.productId != 0)
                    {
                        var productId = fractionator.productId;

                        AddPlanetaryUsage(productId, planetFactory.planet, fractionator.entityId);
                    }
                }

            if (planetUsage)
                for (int i = 1; i < factorySystem.ejectorCursor; i++)
                {
                    var ejector = factorySystem.ejectorPool[i];
                    if (ejector.id != i) continue;
                    AddPlanetaryUsage(ejector.bulletId, planetFactory.planet, ejector.entityId, true);
                }

            if (planetUsage)
                for (int i = 1; i < factorySystem.siloCursor; i++)
                {
                    var silo = factorySystem.siloPool[i];
                    if (silo.id != i) continue;

                    AddPlanetaryUsage(silo.bulletId, planetFactory.planet, silo.entityId, true);
                }

            for (int i = 1; i < factorySystem.labCursor; i++)
            {
                var lab = factorySystem.labPool[i];
                if (lab.id != i) continue;

                if (lab.matrixMode)
                {
                    if (planetUsage)
                        foreach (var productId in lab.requires)
                        {
                            AddPlanetaryUsage(productId, planetFactory.planet, lab.entityId, true);
                        }

                    foreach (var productId in lab.products)
                    {
                        if (planetUsage) AddPlanetaryUsage(productId, planetFactory.planet, lab.entityId);
                        else ProductionDeficit.RecordDeficit(productId, lab, planetFactory);
                    }
                }
                else if (lab.researchMode && planetUsage && lab.techId > 0)
                {
                    var techProto = LDB.techs.Select(lab.techId);
                    for (int index = 0; index < techProto.itemArray.Length; ++index)
                    {
                        var item = techProto.Items[index];
                        AddPlanetaryUsage(item, planetFactory.planet, lab.entityId, true);
                    }
                }
            }

            if (planetUsage)
                for (int i = 1; i < planetFactory.powerSystem.genCursor; i++)
                {
                    var generator = planetFactory.powerSystem.genPool[i];
                    if (generator.id != i)
                    {
                        continue;
                    }

                    var isFuelConsumer = generator.fuelHeat > 0 && generator.fuelId > 0 && generator.productId == 0;
                    if ((generator.productId == 0 || generator.productHeat == 0) && !isFuelConsumer)
                    {
                        continue;
                    }

                    if (isFuelConsumer)
                    {
                        // account for fuel consumption by power generator
                        var productId = generator.fuelId;
                        AddPlanetaryUsage(productId, planetFactory.planet, generator.entityId, true);
                    }
                    else
                    {
                        var productId = generator.productId;
                        AddPlanetaryUsage(productId, planetFactory.planet, generator.entityId);
                        if (generator.catalystId > 0)
                        {
                            AddPlanetaryUsage(generator.catalystId, planetFactory.planet, generator.entityId, true);
                        }
                    }
                }
        }

        private void AddPlanetaryUsage(int productId, PlanetData planet, int entityId, bool consumption = false)
        {
            var productionKey = ProductionKey.From(productId, planet.id, entityId);
            var keys = consumption ? _countedConsumers : _countedProducers;
            if (keys.Contains(productionKey))
            {
                return;
            }

            if (!_productionLocations.ContainsKey(productId))
            {
                _productionLocations[productId] = new PlanetaryProductionSummary();
            }

            if (consumption)
                _productionLocations[productId].AddConsumption(planet.displayName, 1);
            else
                _productionLocations[productId].AddProduction(planet.displayName, 1);
            keys.Add(productionKey);
        }
    }
}