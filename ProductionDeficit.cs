using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bottleneck.Util;

namespace Bottleneck
{
    public class ProductionDeficitItem
    {
        public string recipeName;
        public int assemblerCount { get; set; }
        public int lackingPowerCount { get; set; }

        private readonly int[] needed = new int[10];
        private readonly int[] assemblersNeedingCount = new int[10];
        private readonly string[] inputItemNames = new string[10];
        private readonly Dictionary<int, int> inputItemIndex = new();
        private int neededCount;
        public int jammedCount;

        private static Dictionary<int, Dictionary<int, ProductionDeficitItem>> _byItemByRecipeId = new();

        public void AddNeeded(int itemId, int count)
        {
            if (inputItemIndex.ContainsKey(itemId))
            {
                needed[inputItemIndex[itemId]] += count;
                assemblersNeedingCount[inputItemIndex[itemId]]++;
            }
        }

        public string[] TopNeeded()
        {
            if (assemblerCount < 1)
            {
                return new[] { "", "", "" };
            }

            var neededMax = int.MinValue;
            var neededName = "";
            var secondNeededName = "";
            var assemblerNeedingCount = int.MinValue;
            var secondAssemblerNeedingCount = int.MinValue;

            for (int i = 0; i < neededCount; i++)
            {
                if (needed[i] > neededMax)
                {
                    neededMax = needed[i];
                    secondNeededName = neededName;
                    neededName = inputItemNames[i];
                    secondAssemblerNeedingCount = assemblerNeedingCount;
                    assemblerNeedingCount = assemblersNeedingCount[i];
                }
            }

            var percent = (double)assemblerNeedingCount / assemblerCount;
            var stackingPercent = (double)jammedCount / assemblerCount;
            var neededStr = percent < 0.01 ? "" : $"{neededName} {percent:P2}";
            var stackingStr = stackingPercent < 0.01 ? "" : $"{stackingPercent:P2}";
            var unpoweredPercent = (double)lackingPowerCount / assemblerCount;
            var unpoweredstr = unpoweredPercent < 0.01 ? "" : $"{unpoweredPercent:P2}";
            if (secondNeededName.Length > 0 && (double)secondAssemblerNeedingCount / assemblerCount > 0.01)
            {
                return new[] { $"{neededStr} (2nd: {secondNeededName})".Trim(), $"{stackingStr}".Trim(), unpoweredstr };
            }

            return new[] { $"{neededStr}".Trim(), $"{stackingStr}".Trim(), unpoweredstr };
        }

        public static List<ProductionDeficitItem> ForItemId(int itemId)
        {
            if (!_byItemByRecipeId.ContainsKey(itemId))
            {
                return new List<ProductionDeficitItem>();
            }

            return _byItemByRecipeId[itemId].Values.ToList();
        }

        public static ProductionDeficitItem FromItem(int itemId, AssemblerComponent assemblerComponent)
        {
            var recipeId = assemblerComponent.recipeId;
            _byItemByRecipeId.TryGetValue(itemId, out Dictionary<int, ProductionDeficitItem> byRecipe);
            if (byRecipe == null)
            {
                _byItemByRecipeId[itemId] = byRecipe = new Dictionary<int, ProductionDeficitItem>();
            }

            byRecipe.TryGetValue(recipeId, out ProductionDeficitItem value);
            if (value == null)
            {
                value = new ProductionDeficitItem
                {
                    neededCount = assemblerComponent.requires.Length,
                    recipeName = LDB.recipes.Select(recipeId).Name.Translate()
                };
                for (int i = 0; i < value.neededCount; i++)
                {
                    var requiredItem = LDB.items.Select(assemblerComponent.requires[i]);
                    value.inputItemNames[i] = requiredItem.Name.Translate();
                    value.inputItemIndex[assemblerComponent.requires[i]] = i;
                }

                byRecipe[recipeId] = value;
            }

            return value;
        }

        public static ProductionDeficitItem FromItem(int itemId, LabComponent assemblerComponent)
        {
            var recipeId = assemblerComponent.recipeId;
            _byItemByRecipeId.TryGetValue(itemId, out Dictionary<int, ProductionDeficitItem> byRecipe);
            if (byRecipe == null)
            {
                _byItemByRecipeId[itemId] = byRecipe = new Dictionary<int, ProductionDeficitItem>();
            }

            byRecipe.TryGetValue(recipeId, out ProductionDeficitItem value);
            if (value == null)
            {
                value = new ProductionDeficitItem
                {
                    neededCount = assemblerComponent.requires.Length,
                    recipeName = LDB.recipes.Select(recipeId).Name.Translate()
                };
                for (int i = 0; i < value.neededCount; i++)
                {
                    var requiredItem = LDB.items.Select(assemblerComponent.requires[i]);
                    value.inputItemNames[i] = requiredItem.Name.Translate();
                    value.inputItemIndex[assemblerComponent.requires[i]] = i;
                }

                byRecipe[recipeId] = value;
            }

            return value;
        }

        private void Clear()
        {
            Array.Clear(needed, 0, needed.Length);
            Array.Clear(assemblersNeedingCount, 0, assemblersNeedingCount.Length);

            assemblerCount = 0;
            jammedCount = 0;
            lackingPowerCount = 0;
        }

        public static void ClearCounts()
        {
            foreach (var itemId in _byItemByRecipeId.Keys)
            {
                var productionDeficitItems = ForItemId(itemId);
                foreach (var deficitItem in productionDeficitItems)
                {
                    deficitItem.Clear();
                }
            }
        }
    }

    public static class ProductionDeficit
    {
        public static void Clear()
        {
            ProductionDeficitItem.ClearCounts();
        }

        public static string MostNeeded(int recipeProductId)
        {
            var result = new StringBuilder();
            var productionDeficitItems = ProductionDeficitItem.ForItemId(recipeProductId);
            foreach (var deficitItem in productionDeficitItems)
            {
                var topNeededAry = deficitItem.TopNeeded();
                var neededStr = topNeededAry[0];
                var stackingStr = topNeededAry[1];
                var unpoweredStr = topNeededAry[2];

                if (neededStr.Length == 0 && stackingStr.Length == 0 && unpoweredStr.Length == 0)
                    continue;
                if (result.Length > 0)
                    result.Append("\r\n");
                var tmpResultStr = new StringBuilder();
                if (neededStr.Length > 0)
                {
                    tmpResultStr.Append($"Need: {neededStr}");
                    if (stackingStr.Length > 0)
                        tmpResultStr.Append($", Stacking: {stackingStr}");
                    if (unpoweredStr.Length > 0)
                        tmpResultStr.Append($", Under powered: {unpoweredStr}");
                }
                else if (stackingStr.Length > 0)
                {
                    tmpResultStr.Append($"Stacking: {stackingStr}");
                    if (unpoweredStr.Length > 0)
                        tmpResultStr.Append($", Under powered: {unpoweredStr}");
                }
                else
                {
                    tmpResultStr.Append($"Under powered: {unpoweredStr}");
                }

                if (productionDeficitItems.Count > 1)
                {
                    result.Append($"Recipe: {deficitItem.recipeName}, {tmpResultStr}");
                }
                else
                {
                    result.Append(tmpResultStr);
                }
            }

            return result.ToString();
        }

        private static readonly HashSet<int> _loggedLowPowerByPlanetId = new();

        public static void RecordDeficit(int itemId, AssemblerComponent assembler, PlanetFactory planetFactory)
        {
            var item = ProductionDeficitItem.FromItem(itemId, assembler);
            PowerConsumerComponent consumerComponent = planetFactory.powerSystem.consumerPool[assembler.pcId];
            int networkId = consumerComponent.networkId;
            PowerNetwork powerNetwork = planetFactory.powerSystem.netPool[networkId];
            float ratio = powerNetwork == null || networkId <= 0 ? 0.0f : (float)powerNetwork.consumerRatio;
            if (ratio < 0.99f)
            {
                item.lackingPowerCount++;
                if (!_loggedLowPowerByPlanetId.Contains(planetFactory.planet.id))
                {
                    Log.Debug($"planet is low on power {planetFactory.planet.displayName}");
                    _loggedLowPowerByPlanetId.Add(planetFactory.planet.id);
                }
            }

            item.assemblerCount++;
            for (int index = 0; index < assembler.requireCounts.Length; ++index)
            {
                if (assembler.served[index] < assembler.requireCounts[index])
                {
                    item.AddNeeded(assembler.requires[index], Math.Max(1, assembler.needs[index]));
                }
            }

            for (int i = 0; i < assembler.products.Length; i++)
            {
                if (!assembler.outputing)
                {
                    continue;
                }

                item.jammedCount++;
                break;
            }
        }

        public static void RecordDeficit(int itemId, LabComponent assembler, PlanetFactory planetFactory)
        {
            var item = ProductionDeficitItem.FromItem(itemId, assembler);
            item.assemblerCount++;
            PowerConsumerComponent consumerComponent = planetFactory.powerSystem.consumerPool[assembler.pcId];
            int networkId = consumerComponent.networkId;
            PowerNetwork powerNetwork = planetFactory.powerSystem.netPool[networkId];
            float ratio = powerNetwork == null || networkId <= 0 ? 0.0f : (float)powerNetwork.consumerRatio;
            if (ratio < 0.99f)
            {
                item.lackingPowerCount++;
                if (!_loggedLowPowerByPlanetId.Contains(planetFactory.planet.id))
                {
                    Log.Debug($"planet is low on power {planetFactory.planet.displayName}");
                    _loggedLowPowerByPlanetId.Add(planetFactory.planet.id);
                }
            }

            for (int k = 0; k < assembler.requires.Length; k++)
            {
                if (assembler.served[k] < assembler.requireCounts[k])
                {
                    item.AddNeeded(assembler.requires[k], Math.Max(1, assembler.needs[k]));
                }
            }

            for (int i = 0; i < assembler.products.Length; i++)
            {
                if (assembler.outputing)
                {
                    item.jammedCount++;
                    break;
                }
            }
        }
    }
}